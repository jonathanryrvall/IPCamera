using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using RtspClientSharp.RawFrames.Video;
using SimpleRtspPlayer.RawFramesDecoding.DecodedFrames;

namespace SimpleRtspPlayer.RawFramesDecoding.FFmpeg
{
    class FFmpegVideoDecoder
    {
        private readonly IntPtr _decoderHandle;
        private readonly FFmpegVideoCodecId _videoCodecId;

        private DecodedVideoFrameParameters _currentFrameParameters =
            new DecodedVideoFrameParameters(0, 0, FFmpegPixelFormat.None);

   

        private byte[] _extraData = new byte[0];
        private bool _disposed;

        private FFmpegVideoDecoder(FFmpegVideoCodecId videoCodecId, IntPtr decoderHandle)
        {
            _videoCodecId = videoCodecId;
            _decoderHandle = decoderHandle;
        }

        ~FFmpegVideoDecoder()
        {
            Dispose();
        }

        public static FFmpegVideoDecoder CreateDecoder(FFmpegVideoCodecId videoCodecId)
        {
            int resultCode = FFmpegVideoPInvoke.CreateVideoDecoder(videoCodecId, out IntPtr decoderPtr);

            if (resultCode != 0)
                throw new Exception(
                    $"An error occurred while creating video decoder for {videoCodecId} codec, code: {resultCode}");

            return new FFmpegVideoDecoder(videoCodecId, decoderPtr);
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

                            if (resultCode != 0)
                                throw new Exception(
                                    $"An error occurred while setting video extra data, {_videoCodecId} codec, code: {resultCode}");
                        }
                    }
                }

                resultCode = FFmpegVideoPInvoke.DecodeFrame(_decoderHandle, (IntPtr)rawBufferPtr,
                    rawVideoFrame.FrameSegment.Count,
                    out int width, out int height, out FFmpegPixelFormat pixelFormat);

                if (resultCode != 0)
                    return null;

                if (_currentFrameParameters.Width != width || _currentFrameParameters.Height != height ||
                    _currentFrameParameters.PixelFormat != pixelFormat)
                {
                    _currentFrameParameters = new DecodedVideoFrameParameters(width, height, pixelFormat);
           
                }

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
                videoscaler = CreateScaler(_currentFrameParameters);
            }

            int resultCode = FFmpegVideoPInvoke.ScaleDecodedVideoFrame(_decoderHandle, videoscaler, buffer, bufferStride);
     }





        public IntPtr videoscaler;

     

       

        /// <exception cref="DecoderException"></exception>
        public  IntPtr CreateScaler(DecodedVideoFrameParameters decodedVideoFrameParameters)
        {
            int width = decodedVideoFrameParameters.Width;
            int height = decodedVideoFrameParameters.Height;
            IntPtr scaler;

            FFmpegVideoPInvoke.CreateVideoScaler(0, 0, width, height,
                decodedVideoFrameParameters.PixelFormat,
                width, height, FFmpegPixelFormat.BGRA, FFmpegScalingQuality.FastBilinear, out scaler);

            return scaler;
        }
    }
}