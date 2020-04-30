using System;

namespace SimpleRtspPlayer.RawFramesDecoding.FFmpeg
{
    class FFmpegDecodedVideoScaler
    {
        private bool _disposed;

        public IntPtr Handle { get; }
     
        private FFmpegDecodedVideoScaler(IntPtr handle)
        {
            Handle = handle;
        }

        ~FFmpegDecodedVideoScaler()
        {
            Dispose();
        }

        /// <exception cref="DecoderException"></exception>
        public static FFmpegDecodedVideoScaler Create(DecodedVideoFrameParameters decodedVideoFrameParameters)
        {
         

            int sourceLeft = 0;
            int sourceTop = 0;
            int width = decodedVideoFrameParameters.Width;
            int height = decodedVideoFrameParameters.Height;
            



            FFmpegPixelFormat scaledFFmpegPixelFormat = FFmpegPixelFormat.BGRA;
            FFmpegScalingQuality scaleQuality = FFmpegScalingQuality.FastBilinear;

            int resultCode = FFmpegVideoPInvoke.CreateVideoScaler(sourceLeft, sourceTop, width, height,
                decodedVideoFrameParameters.PixelFormat,
                width, height, scaledFFmpegPixelFormat, scaleQuality, out var handle);

            if (resultCode != 0)
                throw new Exception(@"An error occurred while creating scaler, code: {resultCode}");

            return new FFmpegDecodedVideoScaler(handle);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            FFmpegVideoPInvoke.RemoveVideoScaler(Handle);
            GC.SuppressFinalize(this);
        }

    }
}