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
                SaveFrame(frame);
            }
        }

        /// <summary>
        /// Save a frame!
        /// </summary>
        private void SaveFrame(ImageFrame frame)
        {
            // Create bitmap first time
            //if (bitmap == null)
            //{
                
            //}
            var bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);
            // Write pixels to bitmap
            bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.Data, frame.Width * 4, 0);

            // Save bitmap as png image
            string fileName = path + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
            using (FileStream stream5 = new FileStream(fileName, FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(BitmapFrame.Create(bitmap));
                encoder5.Save(stream5);
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
