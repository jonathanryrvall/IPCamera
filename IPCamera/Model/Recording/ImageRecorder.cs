using IPCamera.Model.Recording;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IPCamera.Model.Recording
{
    /// <summary>
    /// Records frames as png images
    /// </summary>
    public class ImageRecorder : IRecorder
    {
        private bool active;
        private string path;

        /// <summary>
        /// Assign a video source
        /// </summary>
        public void Setup(VideoSource videoSource, string path)
        {
            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            this.path = path;
        }


        /// <summary>
        /// Recieved new frame from video source
        /// </summary>
        private void VideoSource_DecodedFrameReceived(object sender, ImageFrame frame)
        {
            if (active)
            {
                FrameSaver.Save(frame, FrameSaver.GetTimestampFilename());
            }
        }


        /// <summary>
        /// Start recording
        /// </summary>
        public void Start()
        {
            active = true;
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void Stop()
        {
            active = false;
        }

        
    }
}
