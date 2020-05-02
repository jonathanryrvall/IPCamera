//using Accord.Video;
//using Accord.Video.FFMPEG;
//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Media.Imaging;

//namespace IPCamera.Model.Recording
//{
//    public class VideoRecorder : Recorder
//    {
//        private VideoFileWriter writer;
//        public Bitrate Bitrate = Bitrate.K8000;


//        protected override void OpenRecording()
//        {
//            writer = new VideoFileWriter();
        
//            writer.Open(GetFileName(), width, height, 8, VideoCodec.MPEG4, (int)Bitrate);
            

//        }

//        private string GetFileName()
//        {
//            return FilePaths.RecordPath() + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".mp4";
//        }

//        protected override void SaveFrame(ImageFrame frame)
//        {
//            // Create a new bitmap.
//            Bitmap bmp = new Bitmap(frame.Width, frame.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

//            // Lock the bitmap's bits.  
//            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
//            System.Drawing.Imaging.BitmapData bmpData =
//                bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
//                bmp.PixelFormat);

//            // Get the address of the first line.
//            IntPtr ptr = bmpData.Scan0;

//            // Declare an array to hold the bytes of the bitmap.
//            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;

//            // Copy the RGB values back to the bitmap
//            System.Runtime.InteropServices.Marshal.Copy(frame.Data, 0, ptr, bytes);

//            // Unlock the bits.
//            bmp.UnlockBits(bmpData);

//            writer.WriteVideoFrame(bmp);

//        }

//        /// <summary>
//        /// Close writing to file
//        /// </summary>
//        protected override void CloseRecording()
//        {
//            writer.Close();
//            writer.Dispose();
//            writer = null;
//        }
//    }
//}
