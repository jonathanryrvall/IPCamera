using Accord.Video.FFMPEG;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace IPCamera.Model.Recording
{
    public class Recorder
    {
        protected string path;
        protected DateTime ShutdownTime;
        private VideoFileWriter writer;
        public bool IsRecording;
        private TimeSpan recordTime;
        private List<ImageFrame> cachedFrames = new List<ImageFrame>();
        private int preRecord;
        protected int width;
        protected int height;


        /// <summary>
        /// Start recording by setting the shutdown time in the future
        /// </summary>
        public void Start()
        {
            // Open recording if its now a renewal of a ongoing recording
            if (!IsRecording)
            {
                OpenRecording();
            }

            // Extend the shutdown time
            IsRecording = true;
            ShutdownTime = DateTime.Now + recordTime;
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void Stop()
        {
            IsRecording = false;
            CloseRecording();
        }

        /// <summary>
        /// Setup recorder with a video source and a path to record to
        /// </summary>
        public void Setup(VideoSource videoSource, 
                          string path,
                          int preRecord,
                          int recordTime)
        {
            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            this.path = path;
            this.preRecord = preRecord;
            this.recordTime = TimeSpan.FromSeconds(recordTime);
        }


        /// <summary>
        /// Recieve a frame
        /// </summary>
        private void VideoSource_DecodedFrameReceived(object sender, ImageFrame frame)
        {
            width = frame.Width;
            height = frame.Height;

           

            // Recording > Save frames to file
            if (IsRecording)
            {
                // Save any cached frames
                if (cachedFrames.Count > 0)
                {
                    foreach(var cachedFrame in cachedFrames)
                    {
                        SaveFrame(cachedFrame);
                    }
                    cachedFrames.Clear();
                }

                // Save current frame
                SaveFrame(frame);
            }

            // Not recording > cache frames
            else
            {
                // Add to cached
                cachedFrames.Add(frame);

                // Remove when cached is too big
                if (cachedFrames.Count > preRecord)
                {
                    cachedFrames.Remove(cachedFrames.First());
                }
            }

            // Stop recording
            if (TimeUp && IsRecording)
            {
                Stop();
            }
        }


        /// <summary>
        /// Returns true if recording is active
        /// </summary>
        private bool TimeUp => ShutdownTime <= DateTime.Now;


       
        public Bitrate Bitrate = Bitrate.K8000;


        private void OpenRecording()
        {
            writer = new VideoFileWriter();

            writer.Open(GetFileName(), width, height, 8, VideoCodec.MPEG4, (int)Bitrate);


        }

        private string GetFileName()
        {
            return FilePaths.RecordPath() + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".mp4";
        }

        private void SaveFrame(ImageFrame frame)
        {
            // Create a new bitmap.
            Bitmap bmp = new Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(frame.Data, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            writer.WriteVideoFrame(bmp);

        }

        /// <summary>
        /// Close writing to file
        /// </summary>
        private void CloseRecording()
        {
            writer.Close();
            writer.Dispose();
            writer = null;
        }

    }
}
