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

        // 
        private ImageFrame referenceFrame;
        private ImageFrame resultFrame;
   
        private DateTime LastRemodel;
        
        // Output settings
        public ResultMode ResultMode;

        // Settings
        private TimeSpan remodelInterval;
        private byte remodelStrength;
        private int minActiveBlocks;
        private int blockThreshold;
        private int blockSize;
        private byte hotspotThreshold;

        /// <summary>
        /// Initializes a new instance of <see cref="MotionDetector"/>
        /// </summary>
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
            int blockCount = pixelCount / (blockSize * blockSize);
            int blockGridStride = newFrame.Width / blockSize;
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

            // Determine if reference frame should be remodelled this time
            bool doRemodel = DateTime.Now > LastRemodel + remodelInterval;
            if (doRemodel)
            {
                LastRemodel = DateTime.Now;
            }

            // Iterate through all pixels and do some calculations
            int p = 0;
            for (int ps = 0; ps < pixelCount * 4; ps += 4, p++)
            {
                byte rNew = newFrame.Data[ps + 2];
                byte gNew = newFrame.Data[ps + 1];
                byte bNew = newFrame.Data[ps + 0];

                byte rOld = referenceFrame.Data[ps + 2];
                byte gOld = referenceFrame.Data[ps + 1];
                byte bOld = referenceFrame.Data[ps + 0];

                // Calculate 
                byte rDiff = rNew > rOld ? (byte)(rNew - rOld) : (byte)(rOld - rNew);
                byte gDiff = gNew > gOld ? (byte)(gNew - gOld) : (byte)(gOld - gNew);
                byte bDiff = bNew > bOld ? (byte)(bNew - bOld) : (byte)(bOld - bNew);

                // 
                bool isHotspot = rDiff > hotspotThreshold ||
                                 gDiff > hotspotThreshold ||
                                 bDiff > hotspotThreshold;


                // Remodel the reference frame
                if (doRemodel)
                {
                    byte remodelR = rDiff > remodelStrength ? remodelStrength : rDiff;
                    byte remodelG = gDiff > remodelStrength ? remodelStrength : gDiff;
                    byte remodelB = bDiff > remodelStrength ? remodelStrength : bDiff;

                    if (rNew > rOld)
                    {
                        referenceFrame.Data[ps + 2] += remodelR;
                    }
                    else
                    {
                        referenceFrame.Data[ps + 2] -= remodelR;
                    }

                    if (gNew > gOld)
                    {
                        referenceFrame.Data[ps + 1] += remodelG;
                    }
                    else
                    {
                        referenceFrame.Data[ps + 1] -= remodelG;
                    }

                    if (bNew > bOld)
                    {
                        referenceFrame.Data[ps + 0] += remodelB;
                    }
                    else
                    {
                        referenceFrame.Data[ps + 0] -= remodelB;
                    }
                }


                // Output difference as result map
                if (ResultMode == ResultMode.Diff)
                {
                    resultFrame.Data[ps + 0] = rDiff;
                    resultFrame.Data[ps + 1] = gDiff;
                    resultFrame.Data[ps + 2] = bDiff;
                }

                // Output difference after threshold filter as result map
                else if (ResultMode == ResultMode.Threshold)
                {
                    resultFrame.Data[ps + 0] = 0;
                    resultFrame.Data[ps + 1] = 0;
                    resultFrame.Data[ps + 2] = isHotspot ? (byte)255 : (byte)0;
                }

                // Difference after threshold filter applied on the current frame as result map
                else if (ResultMode == ResultMode.Combined)
                {
                    byte v = (byte)(rNew / 2);
                    resultFrame.Data[ps + 0] = v;
                    resultFrame.Data[ps + 1] = v;
                    resultFrame.Data[ps + 2] = isHotspot ? byte.MaxValue : v;
                }

                // Count hotspots
                if (isHotspot)
                {
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    // This cal partially be precalculated to save time
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    int x = p % newFrame.Width;
                    int y = (p - x) / newFrame.Width;
                    int bX = x / blockSize;
                    int bY = y / blockSize;
                    int bI = (bY * blockGridStride) + bX;
                    blocks[bI]++;
                }

            }

            // Write active blocks as bitmap
            if (ResultMode == ResultMode.Blocks)
            {
                p = 0;
                for (int ps = 0; ps < pixelCount * 4; ps += 4, p++)
                {
                    int x = p % newFrame.Width;
                    int y = (p - x) / newFrame.Width;
                    int bX = x / blockSize;
                    int bY = y / blockSize;
                    int bI = (bY * blockGridStride) + bX;

                    resultFrame.Data[ps + 0] = 0;
                    resultFrame.Data[ps + 1] = 0;
                    resultFrame.Data[ps + 2] = blocks[bI] > blockThreshold ? (byte)255 : (byte)0;
                }
            }

            // Write active blocks onto current frame
            if (ResultMode == ResultMode.BlocksCombined)
            {
                p = 0;
                for (int ps = 0; ps < pixelCount * 4; ps += 4, p++)
                {
                    byte rNew = newFrame.Data[ps + 2];
                    byte v = (byte)(rNew / 2);

                    int x = p % newFrame.Width;
                    int y = (p - x) / newFrame.Width;
                    int bX = x / blockSize;
                    int bY = y / blockSize;
                    int bI = (bY * blockGridStride) + bX;

                    resultFrame.Data[ps + 0] = v;
                    resultFrame.Data[ps + 1] = v;
                    resultFrame.Data[ps + 2] = blocks[bI] > blockThreshold ? (byte)255 : v;
                }
            }

            // Count blocks
            int activeBlockCount = blocks.Count(b => b > blockThreshold);


            // Create result to return
            MotionDetectionResult result = new MotionDetectionResult();
            if (ResultMode == ResultMode.Reference)
            {
                result.ResultFrame = referenceFrame;
            }
            else
            {
                result.ResultFrame = resultFrame;
            }

          
            result.ActiveBlocksCount = activeBlockCount;
             result.Motion = result.ActiveBlocksCount >= minActiveBlocks;
            result.ImageFrame = newFrame;
            return result;
        }
    }
}
