using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RtspClientSharp;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using RtspClientSharp.Rtsp;

namespace IPCamera.Model
{
    public class VideoSource
    {
        public IntPtr scalerHandle;
        private IntPtr decoderHandle;
        private byte[] extraData = new byte[0];
        private readonly ConnectionParameters connectionParameters;
        private Task workTask = Task.CompletedTask;
        private CancellationTokenSource cancellationTokenSource;

        private int width;
        private int height;
        private FFmpegPixelFormat PixelFormat;

        public event EventHandler<ImageFrame> DecodedFrameReceived;
        public event EventHandler<string> ConnectionStatusChanged;

        private IntPtr bitmap;
        private int stride;
     
    

        public VideoSource (Config.Config config)
        {
            var deviceUri = new Uri(config.ConnectionStrinng, UriKind.Absolute);
            connectionParameters = new ConnectionParameters(deviceUri);

            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            Start();
        }

        /// <summary>
        /// Start connection to camera
        /// </summary>
        private void Start()
        {
            cancellationTokenSource = new CancellationTokenSource();

            CancellationToken token = cancellationTokenSource.Token;

            workTask = workTask.ContinueWith(async p =>
            {
                await ReceiveAsync(token);
            }, token);
        }

        /// <summary>
        /// Stop camera connection
        /// </summary>
        public void Stop()
        {
            cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// Éndless recieve loop
        /// </summary>
        private async Task ReceiveAsync(CancellationToken token)
        {
            TimeSpan RetryDelay = TimeSpan.FromSeconds(5);

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

        /// <summary>
        /// Frame has been recieved
        /// </summary>
        private void RtspClientOnFrameReceived(object sender, RawFrame rawFrame)
        {
            if (rawFrame is RawVideoFrame)
            {
                ProcessRawFrame(rawFrame as RawVideoFrame);
            }
        }

        /// <summary>
        /// Connection status changed
        /// </summary>
        private void OnStatusChanged(string status)
        {
            ConnectionStatusChanged?.Invoke(this, status);
        }


        /// <summary>
        /// Process the raw frame recieved
        /// </summary>
        private void ProcessRawFrame(RawVideoFrame rawVideoFrame)
        {
            // Make sure theres is a decoder available
            if (decoderHandle == IntPtr.Zero)
            {
                FFmpegVideoCodecId codecId = DetectCodecId(rawVideoFrame);
                FFmpegVideoPInvoke.CreateVideoDecoder(codecId, out decoderHandle);
            }

            // Decode
            if (TryDecode(rawVideoFrame))
            {
                
                if (bitmap == IntPtr.Zero)
                {
                    bitmap = Marshal.AllocHGlobal(width * height * 4);
                    stride = width * 4;
                }


                TransformTo(bitmap, stride);
             
                var outData = new byte[width * height * 4];
                Marshal.Copy(bitmap, outData, 0, outData.Length);

                DecodedFrameReceived?.Invoke(this, new ImageFrame() { Data = outData, Width = width, Height = height });
            }
        }


        /// <summary>
        /// Return codec id based on frame type
        /// </summary>
        private FFmpegVideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return FFmpegVideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return FFmpegVideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }


        /// <summary>
        /// Decode the video frame
        /// </summary>
        public unsafe bool TryDecode(RawVideoFrame rawVideoFrame)
        {
            fixed (byte* rawBufferPtr = &rawVideoFrame.FrameSegment.Array[rawVideoFrame.FrameSegment.Offset])
            {
                int resultCode;

                if (rawVideoFrame is RawH264IFrame rawH264IFrame)
                {
                    if (rawH264IFrame.SpsPpsSegment.Array != null &&
                        !extraData.SequenceEqual(rawH264IFrame.SpsPpsSegment))
                    {
                        if (extraData.Length != rawH264IFrame.SpsPpsSegment.Count)
                            extraData = new byte[rawH264IFrame.SpsPpsSegment.Count];

                        Buffer.BlockCopy(rawH264IFrame.SpsPpsSegment.Array, rawH264IFrame.SpsPpsSegment.Offset,
                            extraData, 0, rawH264IFrame.SpsPpsSegment.Count);

                        fixed (byte* initDataPtr = &extraData[0])
                        {
                            resultCode = FFmpegVideoPInvoke.SetVideoDecoderExtraData(decoderHandle,
                                (IntPtr)initDataPtr, extraData.Length);


                        }
                    }
                }

                resultCode = FFmpegVideoPInvoke.DecodeFrame(decoderHandle, (IntPtr)rawBufferPtr,
                    rawVideoFrame.FrameSegment.Count,
                    out int width, out int height, out FFmpegPixelFormat pixelFormat);

                if (resultCode != 0)
                    return false;

                this.width = width;
                this.height = height;
                PixelFormat = pixelFormat;



                return true;
            }
        }

        /// <summary>
        /// Dispose this object
        /// </summary>
        public void Dispose()
        {
            FFmpegVideoPInvoke.RemoveVideoDecoder(decoderHandle);
            FFmpegVideoPInvoke.RemoveVideoScaler(scalerHandle);
        }


        /// <summary>
        /// Apply the new changes to a writeable bitmap
        /// </summary>
        public void TransformTo(IntPtr bitmap, int stride)
        {
            if (scalerHandle == IntPtr.Zero)
            {
                FFmpegVideoPInvoke.CreateVideoScaler(0, 0, width, height, PixelFormat,
               width, height, FFmpegPixelFormat.BGRA, FFmpegScalingQuality.FastBilinear, out scalerHandle);
            }

            FFmpegVideoPInvoke.ScaleDecodedVideoFrame(decoderHandle, scalerHandle, bitmap, stride);
        }



    }
}