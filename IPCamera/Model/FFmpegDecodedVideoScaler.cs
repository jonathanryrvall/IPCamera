using System;

namespace SimpleRtspPlayer.RawFramesDecoding.FFmpeg
{
    class FFmpegDecodedVideoScaler
    {
        private bool _disposed;

        public IntPtr Handle { get; }
     
        private FFmpegDecodedVideoScaler(IntPtr handle, int scaledWidth, int scaledHeight,
            FFmpegPixelFormat scaledPixelFormat)
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
            int sourceWidth = decodedVideoFrameParameters.Width;
            int sourceHeight = decodedVideoFrameParameters.Height;
            int scaledWidth = decodedVideoFrameParameters.Width;
            int scaledHeight = decodedVideoFrameParameters.Height;




            FFmpegPixelFormat scaledFFmpegPixelFormat = FFmpegPixelFormat.BGRA;
            FFmpegScalingQuality scaleQuality = FFmpegScalingQuality.FastBilinear;

            int resultCode = FFmpegVideoPInvoke.CreateVideoScaler(sourceLeft, sourceTop, sourceWidth, sourceHeight,
                decodedVideoFrameParameters.PixelFormat,
                scaledWidth, scaledHeight, scaledFFmpegPixelFormat, scaleQuality, out var handle);

            if (resultCode != 0)
                throw new DecoderException(@"An error occurred while creating scaler, code: {resultCode}");

            return new FFmpegDecodedVideoScaler(handle, scaledWidth, scaledHeight, scaledFFmpegPixelFormat);
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