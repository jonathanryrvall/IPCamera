using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.MotionDetection
{
    /// <summary>
    /// Detects motion
    /// </summary>
    public class MotionDetector
    {
        public event EventHandler<MotionDetectionResult> OnMotionDetectionResult;

        private ImageFrame lastFrame = null;

        private bool outputResult;
        private byte hotspotThreshold;
        private byte[] result;
        private double maxHotspots;


        public MotionDetector(VideoSource videoSource,
                              Config.Config config)
        {
            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            hotspotThreshold = config.HotSpotThreshold;
            maxHotspots = config.MaxHotSpots;
        }

        /// <summary>
        /// Recieved frame from video source
        /// </summary>
        private void VideoSource_DecodedFrameReceived(object sender, ImageFrame e)
        {
            DetectMotion(e);
        }

        /// <summary>
        /// Try to detect motion in a frame
        /// </summary>
        private void DetectMotion(ImageFrame frame)
        {
            if (lastFrame != null)
            {
                var det = RunKernel(frame, lastFrame);
                OnMotionDetectionResult?.Invoke(this, det);
            }

            lastFrame = frame;
        }

        /// <summary>
        /// Count how many pixels exceeds threshold
        /// </summary>
        private MotionDetectionResult RunKernel(ImageFrame newFrame,
                                 ImageFrame oldFrame)
        {
            const int maxDiff = 100;
            int pixelCount = newFrame.Width * newFrame.Height;
            int hotSpotCount = 0;

            if (result == null)
            {
                result = new byte[pixelCount * 4];
            }



            for (int p = 0; p < pixelCount; p++)
            {
                int rNew = newFrame.Data[p * 4 + 2];
                int gNew = newFrame.Data[p * 4 + 1];
                int bNew = newFrame.Data[p * 4 + 0];

                int rOld = oldFrame.Data[p * 4 + 2];
                int gOld = oldFrame.Data[p * 4 + 1];
                int bOld = oldFrame.Data[p * 4 + 0];

                if (rNew - rOld > maxDiff ||
                    gNew - gOld > maxDiff ||
                    bNew - bOld > maxDiff)
                {
                    hotSpotCount++;
                    result[p * 4 + 0] = 255;
                    result[p * 4 + 1] = 0;
                    result[p * 4 + 2] = 0;
                    result[p * 4 + 3] = 255;

                }
                else
                {
                    result[p * 4 + 0] = 255;
                    result[p * 4 + 1] = 255;
                    result[p * 4 + 2] = 255;
                    result[p * 4 + 3] = 255;
                }
            }

            bool isMotion = hotspotThreshold > ((double)pixelCount * maxHotspots);

                return new MotionDetectionResult() { Bitmap = result, Hotspots = hotSpotCount, Motion = isMotion };
        }
    }
}
