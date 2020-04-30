using System;
using System.Linq;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;
using SimpleRtspPlayer.RawFramesDecoding.DecodedFrames;
using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;

namespace SimpleRtspPlayer.RawFramesReceiving
{
    public class RawFramesSource 
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(5);
        private readonly ConnectionParameters _connectionParameters;
        private Task _workTask = Task.CompletedTask;
        private CancellationTokenSource _cancellationTokenSource;

        public event EventHandler<DecodedVideoFrame> DecodedFrameReceived;

        public EventHandler<RawFrame> FrameReceived { get; set; }
        public EventHandler<string> ConnectionStatusChanged { get; set; }

        public RawFramesSource(ConnectionParameters connectionParameters)
        {
            _connectionParameters =
                connectionParameters ?? throw new ArgumentNullException(nameof(connectionParameters));
        }

        public void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();

            CancellationToken token = _cancellationTokenSource.Token;

            _workTask = _workTask.ContinueWith(async p =>
            {
                await ReceiveAsync(token);
            }, token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task ReceiveAsync(CancellationToken token)
        {
            try
            {
                using (var rtspClient = new RtspClient(_connectionParameters))
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
            OnFrameReceived(sender, rawFrame);
            FrameReceived?.Invoke(this, rawFrame);
        }

        private void OnStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }




















        public IntPtr videoscaler;
        private IntPtr _decoderHandle;
        public int Width;
        public int Height;
        public FFmpegPixelFormat PixelFormat;
        private byte[] _extraData = new byte[0];
        private bool _disposed;

        public void SetRawFramesSource(RawFramesSource rawFramesSource)
        {
            rawFramesSource.FrameReceived += OnFrameReceived;
        }




        private void OnFrameReceived(object sender, RawFrame rawFrame)
        {
            if (!(rawFrame is RawVideoFrame rawVideoFrame))
                return;


            FFmpegVideoCodecId codecId = DetectCodecId(rawVideoFrame);
            if (_decoderHandle == IntPtr.Zero)
            {
                FFmpegVideoPInvoke.CreateVideoDecoder(codecId, out _decoderHandle);

            }


            DecodedVideoFrame decodedFrame = TryDecode(rawVideoFrame);

            if (decodedFrame != null)
                DecodedFrameReceived?.Invoke(this, decodedFrame);
        }



        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }








        public unsafe DecodedVideoFrame TryDecode(RawVideoFrame rawVideoFrame)
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
                            resultCode = FFmpegVideoPInvoke.SetVideoDecoderExtraData(_decoderHandle,
                                (IntPtr)initDataPtr, _extraData.Length);


                        }
                    }
                }

                resultCode = FFmpegVideoPInvoke.DecodeFrame(_decoderHandle, (IntPtr)rawBufferPtr,
                    rawVideoFrame.FrameSegment.Count,
                    out int width, out int height, out FFmpegPixelFormat pixelFormat);

                if (resultCode != 0)
                    return null;

                Width = width;
                Height = height;
                PixelFormat = pixelFormat;



                return new DecodedVideoFrame(TransformTo);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            FFmpegVideoPInvoke.RemoveVideoDecoder(_decoderHandle);
            FFmpegVideoPInvoke.RemoveVideoScaler(videoscaler);

            GC.SuppressFinalize(this);
        }



        private void TransformTo(IntPtr buffer, int bufferStride)
        {
            if (videoscaler == IntPtr.Zero)
            {
                FFmpegVideoPInvoke.CreateVideoScaler(0, 0, Width, Height,
               PixelFormat,
               Width, Height, FFmpegPixelFormat.BGRA, FFmpegScalingQuality.FastBilinear, out videoscaler);
            }

            FFmpegVideoPInvoke.ScaleDecodedVideoFrame(_decoderHandle, videoscaler, buffer, bufferStride);
        }



    }
}