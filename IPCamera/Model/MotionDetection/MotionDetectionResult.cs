using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.MotionDetection
{
    public class MotionDetectionResult
    {
        public byte[] Bitmap;
        public int Hotspots;
        public bool Motion;
    }
}
