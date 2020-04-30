using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;
using System.Drawing;

namespace SimpleRtspPlayer.RawFramesDecoding
{
    public class TransformParameters
    {
     
        public FFmpegPixelFormat TargetFormat { get; }

        public FFmpegScalingQuality ScaleQuality { get; }

        public TransformParameters(        FFmpegPixelFormat targetFormat, FFmpegScalingQuality scaleQuality)
        {
            TargetFormat = targetFormat;
            ScaleQuality = scaleQuality;
      
        }

    }
}