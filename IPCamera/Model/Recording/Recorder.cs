using Accord.Video.FFMPEG;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace IPCamera.Model.Recording
{
    /// <summary>
    /// Records frames to a MP4 file
    /// </summary>
    public class Recorder
    {
        public volatile bool IsRecording;
        
        private int frameRate;

        private DateTime ShutdownTime;
        private TimeSpan recordTime;
        private List<ImageFrame> cachedFrames = new List<ImageFrame>();
        private int preRecord;
        private int width;
        private int height;
        private Bitrate bitrate;

        private Bitmap saveBitmap;
        private BlockingCollection<ImageFrame> queue;

      

        /// <summary>
        /// Start recording by setting the shutdown time in the future
        /// </summary>
        public void Start()
        {
            // Open recording if its now a renewal of a ongoing recording
            if (!IsRecording)
            {
                IsRecording = true;
                new Thread(OpenWriter).Start();
            }

            // Extend the shutdown time
            ShutdownTime = DateTime.Now + recordTime;
        }

        /// <summary>
        /// Stop recording
        /// </summary>
        public void Stop()
        {
            IsRecording = false;
        }

        /// <summary>
        /// Setup recorder with a video source and a path to record to
        /// </summary>
        public Recorder(VideoSource videoSource,
                          int frameRate,
                          int preRecord,
                          int recordTime,
                          Bitrate bitrate)
        {
            queue = new BlockingCollection<ImageFrame>(new ConcurrentQueue<ImageFrame>());

            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            this.frameRate = frameRate;
            this.preRecord = preRecord;
            this.recordTime = TimeSpan.FromSeconds(recordTime);
            this.bitrate = bitrate;
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
                    foreach (var cachedFrame in cachedFrames)
                    {
                        queue.Add(cachedFrame);
                    }
                    cachedFrames.Clear();
                }

                // Save current frame
                queue.Add(frame);
            }

            // Not recording > cache frames
            else if (preRecord > 0)
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

        /// <summary>
        /// Open writing to file in a separate thread polling a queue of frames
        /// </summary>
        private void OpenWriter()
        {
            using (var writer = new VideoFileWriter())
            {
                writer.Open(GetFileName(), width, height, frameRate, VideoCodec.MPEG4, (int)bitrate);

                while (IsRecording)
                {
                    if (queue.TryTake(out ImageFrame frame, 100))
                    {
                        SaveFrame(frame, writer);
                    }
                }

                writer.Close();
            }

        }



        /// <summary>
        /// Get file name for this recording
        /// </summary>
        private string GetFileName()
        {
            return FilePaths.RecordPath() + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".mp4";
        }

        /// <summary>
        /// Save a frame
        /// </summary>
        private void SaveFrame(ImageFrame frame, VideoFileWriter writer)
        {
            // Create a new bitmap.
            if (saveBitmap == null)
            {
                saveBitmap = new Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            }

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, saveBitmap.Width, saveBitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData =
                saveBitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                saveBitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * saveBitmap.Height;

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(frame.Data, 0, ptr, bytes);

            // Unlock the bits.
            saveBitmap.UnlockBits(bmpData);

            writer.WriteVideoFrame(saveBitmap);
        }



    }
}
