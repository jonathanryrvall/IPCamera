using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.MotionDetection
{
    public class MotionDetectionResult
    {
        public ImageFrame ResultFrame;
        public ImageFrame ImageFrame;

        public int HotspotCount;
        public double HotspotPercentage;
        public bool Motion;
    }
}
