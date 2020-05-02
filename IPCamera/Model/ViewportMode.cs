using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model
{
    public enum ViewportMode
    {
        Image,
        MotionDetectionDiff,
        MotionDetectionThreshold,
        CombinedThreshold,
        Reference,
        Blocks
    }
}
