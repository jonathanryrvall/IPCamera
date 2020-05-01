using IPCamera.Model;
using RtspClientSharp;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IPCamera.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainV
    {
        public string DeviceAddress  = "rtsp://admin:admins@192.168.1.101/user=admin_password=_channel=1_stream=0.sdp";

  //      private WriteableBitmap bitmap;
        private VideoSource videoSource;


        public MainV()
        {
            InitializeComponent();
            
          

         //   VideoImage.Source = bitmap;
           
            // Setup camera
            var deviceUri = new Uri(DeviceAddress, UriKind.Absolute);
            var connectionParameters = new ConnectionParameters(deviceUri);
            
            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);


            videoSource = new VideoSource(connectionParameters);
            videoSource.DecodedFrameReceived += OnFrameReceived;
            videoSource.Start();

        }

        WriteableBitmap wbm;



        //public static BitmapSource FromArray(byte[] data, int w, int h, int ch)
        //{
        //    PixelFormat format = PixelFormats.Default;

        //    if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
        //    if (ch == 3) format = PixelFormats.Bgr24; //RGB
        //    if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


        //     wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

        //    return wbm;
        //}


        private void OnFrameReceived(object sender, byte[] e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (wbm == null)
                {
                    wbm = new WriteableBitmap(1280, 720, 96, 96, PixelFormats.Bgr32, null);
                    VideoImage.Source = wbm;
                }

                wbm.WritePixels(new Int32Rect(0, 0, 1280, 720), e, 1280 * 4, 0);

               
               // var clone = e.Clone();

                //VideoImage.Source = e.Clone();


            });
        }

    }
}