using System;
using System.Collections.Generic;
using System.Linq;
using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using SimpleRtspPlayer.RawFramesDecoding.DecodedFrames;
using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;
using SimpleRtspPlayer.RawFramesReceiving;

namespace SimpleRtspPlayer.GUI
{
    public class RealtimeVideoSource
    {
        public IntPtr videoscaler;
        public event EventHandler<DecodedVideoFrame> FrameReceived;
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
                FrameReceived?.Invoke(this, decodedFrame);
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