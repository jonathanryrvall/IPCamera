using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.Recording
{
    public interface IRecorder
    {
        void Setup(VideoSource videoSource, string path);

        void Start();
        void Stop();
    }
}
