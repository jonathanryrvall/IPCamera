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

        private ImageFrame referenceFrame = null;
        private byte hotspotThreshold;
        private ImageFrame resultFrame;

        private double maxHotspots;

        public ResultMode ResultMode;
        private DateTime LastRemodel;
        private TimeSpan remodelInterval;
        private byte remodelStrength;
        private int minActiveBlocks;
        private int blockThreshold;
        private int blockSize;


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
            remodelInterval = TimeSpan.FromMilliseconds(config.RemodelInterval);
            remodelStrength = config.RemodelStrength;
            blockSize = config.BlockSize;
            blockThreshold = config.BlockThreshold;
            minActiveBlocks = config.MinActiveBlocks;
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
            if (referenceFrame != null)
            {
                var det = RunKernel(frame);
                OnMotionDetectionResult?.Invoke(this, det);
            }
            else
            {
                LastRemodel = DateTime.Now;
                referenceFrame = frame;
            }

        }

        /// <summary>
        /// Count how many pixels exceeds threshold
        /// </summary>
        private MotionDetectionResult RunKernel(ImageFrame newFrame)
        {
            int pixelCount = newFrame.Width * newFrame.Height;
            int hotSpotCount = 0;

            int blockCount = (newFrame.Width / blockSize) * (newFrame.Height / blockSize);
            int[] blocks = new int[blockCount];

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

            bool doRemodel = DateTime.Now > LastRemodel + remodelInterval;
            if (doRemodel)
            {
                LastRemodel = DateTime.Now;
            }


            for (int p = 0; p < pixelCount; p++)
            {
               

                byte rNew = newFrame.Data[p * 4 + 2];
                byte gNew = newFrame.Data[p * 4 + 1];
                byte bNew = newFrame.Data[p * 4 + 0];

                byte rOld = referenceFrame.Data[p * 4 + 2];
                byte gOld = referenceFrame.Data[p * 4 + 1];
                byte bOld = referenceFrame.Data[p * 4 + 0];


                byte rDiff = rNew > rOld ? (byte)(rNew - rOld) : (byte)(rOld - rNew);
                byte gDiff = gNew > gOld ? (byte)(gNew - gOld) : (byte)(gOld - gNew);
                byte bDiff = bNew > bOld ? (byte)(bNew - bOld) : (byte)(bOld - bNew);

                bool isHotspot = rDiff > hotspotThreshold ||
                                 gDiff > hotspotThreshold ||
                                 bDiff > hotspotThreshold;

                if (doRemodel)
                {
                    byte remodelR = rDiff > remodelStrength ? remodelStrength : rDiff;
                    byte remodelG = gDiff > remodelStrength ? remodelStrength : gDiff;
                    byte remodelB = bDiff > remodelStrength ? remodelStrength : bDiff;

                    if (rNew > rOld)
                    {
                        referenceFrame.Data[p * 4 + 2] += remodelR;
                    }
                    else
                    {
                        referenceFrame.Data[p * 4 + 2] -= remodelR;
                    }

                    if (gNew > gOld)
                    {
                        referenceFrame.Data[p * 4 + 1] += remodelG;
                    }
                    else
                    {
                        referenceFrame.Data[p * 4 + 1] -= remodelG;
                    }

                    if (bNew > bOld)
                    {
                        referenceFrame.Data[p * 4 + 0] += remodelB;
                    }
                    else
                    {
                        referenceFrame.Data[p * 4 + 0] -= remodelB;
                    }



                }





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

                else if (ResultMode == ResultMode.Combined)
                {
                    byte v = (byte)(rNew / 2);
                    resultFrame.Data[p * 4 + 0] = v;
                    resultFrame.Data[p * 4 + 1] = v;
                    resultFrame.Data[p * 4 + 2] = isHotspot ? byte.MaxValue : v;
                }

                if (isHotspot)
                {
                    hotSpotCount++;

                    int x = p % newFrame.Width;
                    int y = (p - x) / newFrame.Width;
                    int bX = x / blockSize;
                    int bY = y / blockSize;
                    int bI = (bY * (newFrame.Width / blockSize)) + bX;
                    blocks[bI]++;
                }

            }

            if (ResultMode == ResultMode.Blocks)
            {
                for (int p = 0; p < pixelCount; p++)
                {

                    int x = p % newFrame.Width;
                    int y = (p - x) / newFrame.Width;
                    int bX = x / blockSize;
                    int bY = y / blockSize;
                    int bI = (bY * (newFrame.Width / blockSize)) + bX;

                    resultFrame.Data[p * 4 + 0] = 0;
                    resultFrame.Data[p * 4 + 1] = 0;
                    resultFrame.Data[p * 4 + 2] = blocks[bI] > blockThreshold ? (byte)255 : (byte)0;

                }
            }

            if (ResultMode == ResultMode.BlocksCombined)
            {
                for (int p = 0; p < pixelCount; p++)
                {
                    byte rNew = newFrame.Data[p * 4 + 2];
                    byte v = (byte)(rNew / 2);

                    int x = p % newFrame.Width;
                    int y = (p - x) / newFrame.Width;
                    int bX = x / blockSize;
                    int bY = y / blockSize;
                    int bI = (bY * (newFrame.Width / blockSize)) + bX;

                    resultFrame.Data[p * 4 + 0] = v;
                    resultFrame.Data[p * 4 + 1] = v;
                    resultFrame.Data[p * 4 + 2] = blocks[bI] > blockThreshold ? (byte)255 : v;

                }
            }

            // Count blocks
            int activeBlockCount = blocks.Count(b => b > blockThreshold);

            MotionDetectionResult result = new MotionDetectionResult();
            if (ResultMode == ResultMode.Reference)
            {
                result.ResultFrame = referenceFrame;
            }
            else
            {
                result.ResultFrame = resultFrame;
            }

            result.HotspotCount = hotSpotCount;
            result.ActiveBlocksCount = activeBlockCount;
            result.HotspotPercentage = ((double)hotSpotCount / (double)pixelCount) * 100;
            result.Motion = result.ActiveBlocksCount >= minActiveBlocks;
            result.ImageFrame = newFrame;
            return result;
        }
    }
}
