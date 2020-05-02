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
    /// <summary>
    /// Records frames to a MP4 file
    /// </summary>
    public class Recorder
    {
        public bool IsRecording;
        private string path;
        private DateTime ShutdownTime;
        private VideoFileWriter writer;
        private TimeSpan recordTime;
        private List<ImageFrame> cachedFrames = new List<ImageFrame>();
        private int preRecord;
        private int width;
        private int height;
        private Bitrate bitrate;

        private Bitmap saveBitmap;

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
        public Recorder(VideoSource videoSource,
                          string path,
                          int preRecord,
                          int recordTime,
                          Bitrate bitrate)
        {
            videoSource.DecodedFrameReceived += VideoSource_DecodedFrameReceived;
            this.path = path;
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
                        SaveFrame(cachedFrame);
                    }
                    cachedFrames.Clear();
                }

                // Save current frame
                SaveFrame(frame);
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
        /// Ope video writer
        /// </summary>
        private void OpenRecording()
        {
            writer = new VideoFileWriter();
            writer.Open(GetFileName(), width, height, 8, VideoCodec.MPEG4, (int)bitrate);
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
        /// <param name="frame"></param>
        private void SaveFrame(ImageFrame frame)
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
