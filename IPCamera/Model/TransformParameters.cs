using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;
using System.Drawing;

namespace SimpleRtspPlayer.RawFramesDecoding
{
    public class TransformParameters
    {
     
        public Size TargetFrameSize { get; }

     
        public FFmpegPixelFormat TargetFormat { get; }

        public FFmpegScalingQuality ScaleQuality { get; }

        public TransformParameters(Size targetFrameSize,
            FFmpegPixelFormat targetFormat, FFmpegScalingQuality scaleQuality)
        {
            TargetFrameSize = targetFrameSize;
            TargetFormat = targetFormat;
            ScaleQuality = scaleQuality;
      
        }

    }
}