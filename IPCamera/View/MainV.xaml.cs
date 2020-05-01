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

        private VideoSource videoSource;
        private WriteableBitmap wbm;

        public MainV()
        {
            InitializeComponent();
            
           
            // Setup camera
            var deviceUri = new Uri(DeviceAddress, UriKind.Absolute);
            var connectionParameters = new ConnectionParameters(deviceUri);
            
            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);


            videoSource = new VideoSource(connectionParameters);
            videoSource.DecodedFrameReceived += OnFrameReceived;
            videoSource.Start();

        }

       


        private void OnFrameReceived(object sender, ImageFrame frame)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (wbm == null)
                {
                    wbm = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);
                    VideoImage.Source = wbm;
                }

                wbm.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.Data, frame.Width * 4, 0);

       

            });
        }

    }
}