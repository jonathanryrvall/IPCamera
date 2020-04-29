using System.Drawing;

namespace SimpleRtspPlayer.RawFramesDecoding
{
    public class TransformParameters
    {
        public RectangleF RegionOfInterest { get; }

        public Size TargetFrameSize { get; }

        public ScalingPolicy ScalePolicy { get; }

        public PixelFormat TargetFormat { get; }

        public ScalingQuality ScaleQuality { get; }

        public TransformParameters(RectangleF regionOfInterest, Size targetFrameSize, ScalingPolicy scalePolicy,
            PixelFormat targetFormat, ScalingQuality scaleQuality)
        {
            RegionOfInterest = regionOfInterest;
            TargetFrameSize = targetFrameSize;
            TargetFormat = targetFormat;
            ScaleQuality = scaleQuality;
            ScalePolicy = scalePolicy;
        }

    }
}