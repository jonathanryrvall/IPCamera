using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPCamera.Model.Recording
{
    /// <summary>
    /// Creates a timelapse
    /// </summary>
    public class Timelapser
    {
        private DateTime lastSave = new DateTime(2015, 06, 12);
        private TimeSpan interval;

        public Timelapser(VideoSource videoSource,
                          Config.Config config)
        {
            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            interval = TimeSpan.FromMinutes(config.TimelapseIntervalMinutes);
        }

        /// <summary>
        /// Recieve a frame from video source
        /// </summary>
        private void VideoSource_DecodedFrameReceived(object sender, ImageFrame e)
        {
            // Save it if enough time has passed
            if (DateTime.Now > lastSave + interval)
            {
                new Thread(() =>
                {
                    FrameSaver.Save(e, FrameSaver.GetTimestampFilename(FilePaths.TimelapsePath()));
                }).Start();

                lastSave = DateTime.Now;
            }
        }
    }
}
