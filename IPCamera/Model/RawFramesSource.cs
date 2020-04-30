using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;

using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;

namespace SimpleRtspPlayer.RawFramesReceiving
{
    public class RawFramesSource
    {
        public IntPtr scalerHandle;
        private IntPtr decoderHandle;
        public int Width;
        public int Height;
        public FFmpegPixelFormat PixelFormat;
        private byte[] _extraData = new byte[0];
        private bool _disposed;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
        private readonly ConnectionParameters connectionParameters;
        private Task workTask = Task.CompletedTask;
        private CancellationTokenSource cancellationTokenSource;



        public event EventHandler DecodedFrameReceived;
        public event EventHandler<RawFrame> FrameReceived;
        public event EventHandler<string> ConnectionStatusChanged;


        public RawFramesSource(ConnectionParameters connectionParameters)
        {
            this.connectionParameters = connectionParameters;
        }

        public void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();

            CancellationToken token = cancellationTokenSource.Token;

            workTask = workTask.ContinueWith(async p =>
            {
                await ReceiveAsync(token);
            }, token);
        }

        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Éndless recieve loop
        /// </summary>
        private async Task ReceiveAsync(CancellationToken token)
        {
            try
            {
                using (var rtspClient = new RtspClient(connectionParameters))
                {
                    rtspClient.FrameReceived += RtspClientOnFrameReceived;

                    while (true)
                    {
                        OnStatusChanged("Connecting...");

                        try
                        {
                            await rtspClient.ConnectAsync(token);
                        }
                        catch (InvalidCredentialException)
                        {
                            OnStatusChanged("Invalid login and/or password");
                            await Task.Delay(RetryDelay, token);
                            continue;
                        }
                        catch (RtspClientException e)
                        {
                            OnStatusChanged(e.ToString());
                            await Task.Delay(RetryDelay, token);
                            continue;
                        }

                        OnStatusChanged("Receiving frames...");

                        try
                        {
                            await rtspClient.ReceiveAsync(token);
                        }
                        catch (RtspClientException e)
                        {
                            OnStatusChanged(e.ToString());
                            await Task.Delay(RetryDelay, token);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void RtspClientOnFrameReceived(object sender, RawFrame rawFrame)
        {
            ProcessRawFrame(rawFrame);
            FrameReceived?.Invoke(this, rawFrame);
        }

        /// <summary>
        /// Connection status changed
        /// </summary>
        private void OnStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }



        private void ProcessRawFrame(RawFrame rawFrame)
        {
            if (!(rawFrame is RawVideoFrame rawVideoFrame))
                return;


            FFmpegVideoCodecId codecId = DetectCodecId(rawVideoFrame);
            if (decoderHandle == IntPtr.Zero)
            {
                FFmpegVideoPInvoke.CreateVideoDecoder(codecId, out decoderHandle);

            }


            if (TryDecode(rawVideoFrame))
            {
                DecodedFrameReceived?.Invoke(this, null);
            }



        }



        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }








        public unsafe bool TryDecode(RawVideoFrame rawVideoFrame)
        {
            fixed (byte* rawBufferPtr = &rawVideoFrame.FrameSegment.Array[rawVideoFrame.FrameSegment.Offset])
            {
                int resultCode;

                if (rawVideoFrame is RawH264IFrame rawH264IFrame)
                {
                    if (rawH264IFrame.SpsPpsSegment.Array != null &&
                        !_extraData.SequenceEqual(rawH264IFrame.SpsPpsSegment))
                    {
                        if (_extraData.Length != rawH264IFrame.SpsPpsSegment.Count)
                            _extraData = new byte[rawH264IFrame.SpsPpsSegment.Count];

                        Buffer.BlockCopy(rawH264IFrame.SpsPpsSegment.Array, rawH264IFrame.SpsPpsSegment.Offset,
                            _extraData, 0, rawH264IFrame.SpsPpsSegment.Count);

                        fixed (byte* initDataPtr = &_extraData[0])
                        {
                            resultCode = FFmpegVideoPInvoke.SetVideoDecoderExtraData(decoderHandle,
                                (IntPtr)initDataPtr, _extraData.Length);


                        }
                    }
                }

                resultCode = FFmpegVideoPInvoke.DecodeFrame(decoderHandle, (IntPtr)rawBufferPtr,
                    rawVideoFrame.FrameSegment.Count,
                    out int width, out int height, out FFmpegPixelFormat pixelFormat);

                if (resultCode != 0)
                    return false;

                Width = width;
                Height = height;
                PixelFormat = pixelFormat;



                return true;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            FFmpegVideoPInvoke.RemoveVideoDecoder(decoderHandle);
            FFmpegVideoPInvoke.RemoveVideoScaler(scalerHandle);

            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Apply the new changes to a writeable bitmap
        /// </summary>
        public void TransformTo(WriteableBitmap bitmap)
        {
            if (scalerHandle == IntPtr.Zero)
            {
                FFmpegVideoPInvoke.CreateVideoScaler(0, 0, Width, Height,
               PixelFormat,
               Width, Height, FFmpegPixelFormat.BGRA, FFmpegScalingQuality.FastBilinear, out scalerHandle);
            }

            FFmpegVideoPInvoke.ScaleDecodedVideoFrame(decoderHandle, scalerHandle, bitmap.BackBuffer, bitmap.BackBufferStride);
        }



    }
}