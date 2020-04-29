using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;

namespace SimpleRtspPlayer.RawFramesDecoding
{
    class DecodedVideoFrameParameters
    {
        public int Width { get; }

        public int Height { get; }

        public FFmpegPixelFormat PixelFormat { get; }

        public DecodedVideoFrameParameters(int width, int height, FFmpegPixelFormat pixelFormat)
        {
            Width = width;
            Height = height;
            PixelFormat = pixelFormat;
        }

     
    }
}