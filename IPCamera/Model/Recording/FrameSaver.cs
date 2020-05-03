using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IPCamera.Model.Recording
{
    public static class FrameSaver
    {
        public static void Save(ImageFrame frame, string fileName)
        {
            var bitmap = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);
            // Write pixels to bitmap
            bitmap.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.Data, frame.Width * 4, 0);

            // Save bitmap as png image
            using (FileStream stream5 = new FileStream(fileName, FileMode.Create))
            {
                PngBitmapEncoder encoder5 = new PngBitmapEncoder();
                encoder5.Frames.Add(BitmapFrame.Create(bitmap));
                encoder5.Save(stream5);
            }
        }

        public static string GetTimestampFilename(string path)
        {
            return path + "/" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
        }
    }
}
