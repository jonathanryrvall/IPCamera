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
        private byte hotspotThreshold;
        private ImageFrame resultFrame;

        private double maxHotspots;

        public ResultMode ResultMode;

        public MotionDetector(VideoSource videoSource,
                              Config.Config config)
        {
            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            
            UpdateConfig(config);
        }


        /// <summary>
        /// Update the configuration
        /// </summary>
        public void UpdateConfig(Config.Config config)
        {
            hotspotThreshold = config.HotspotThreshold;
            maxHotspots = config.MaxHotspots;
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
            int pixelCount = newFrame.Width * newFrame.Height;
            int hotSpotCount = 0;

            // Init the result frame first time if it has not yet been initialized
            if (resultFrame == null)
            {
                resultFrame = new ImageFrame();
                resultFrame.Data = new byte[pixelCount * 4];
                resultFrame.Width = newFrame.Width;
                resultFrame.Height = newFrame.Height;

                // Init the alpha channel
                for (int i = 0; i < pixelCount; i++)
                {
                    resultFrame.Data[i * 4 + 3] = 255;
                }
            }



            for (int p = 0; p < pixelCount; p++)
            {
                byte rNew = newFrame.Data[p * 4 + 2];
                byte gNew = newFrame.Data[p * 4 + 1];
                byte bNew = newFrame.Data[p * 4 + 0];

                byte rOld = oldFrame.Data[p * 4 + 2];
                byte gOld = oldFrame.Data[p * 4 + 1];
                byte bOld = oldFrame.Data[p * 4 + 0];


                byte rDiff = rNew > rOld ? (byte)(rNew - rOld) : (byte)(rOld - rNew);
                byte gDiff = gNew > gOld ? (byte)(gNew - gOld) : (byte)(gOld - gNew);
                byte bDiff = bNew > bOld ? (byte)(bNew - bOld) : (byte)(bOld - bNew);

                bool isHotspot = rDiff > hotspotThreshold ||
                                 gDiff > hotspotThreshold ||
                                 bDiff > hotspotThreshold;

                if (ResultMode == ResultMode.Diff)
                {
                    resultFrame.Data[p * 4 + 0] = rDiff;
                    resultFrame.Data[p * 4 + 1] = gDiff;
                    resultFrame.Data[p * 4 + 2] = bDiff;
                }

                else if (ResultMode == ResultMode.Threshold)
                {
                    resultFrame.Data[p * 4 + 0] = 0;
                    resultFrame.Data[p * 4 + 1] = 0;
                    resultFrame.Data[p * 4 + 2] = isHotspot ? (byte)255 : (byte)0;
                }

                if (isHotspot)
                {
                    hotSpotCount++;
                }
           
            }

            MotionDetectionResult result = new MotionDetectionResult();
            result.ResultFrame = resultFrame;
            result.HotspotCount = hotSpotCount;
            result.HotspotPercentage = ((double)hotSpotCount / (double)pixelCount) * 100;
            result.Motion = result.HotspotPercentage > maxHotspots;
            result.ImageFrame = newFrame;
            return result;
        }
    }
}
