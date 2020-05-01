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
        public string DeviceAddress { get; set; } = "rtsp://admin:admins@192.168.1.101/user=admin_password=_channel=1_stream=0.sdp";

        public string Login { get; set; } = "admin";
        public string Password { get; set; } = "123456";


        private WriteableBitmap bitmap;



        private VideoSource videoSource;


        public MainV()
        {
            InitializeComponent();
            bitmap = new WriteableBitmap(1280, 720, 96.0, 96.0, PixelFormats.Pbgra32, null);
            RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.NearestNeighbor);

            bitmap.Lock();
            bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
            bitmap.Unlock();


            VideoImage.Source = bitmap;



            if (!Uri.TryCreate(DeviceAddress, UriKind.Absolute, out Uri deviceUri))
            {
                MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }




            var connectionParameters = new ConnectionParameters(deviceUri);
            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            videoSource = new VideoSource(connectionParameters);

            videoSource.DecodedFrameReceived += OnFrameReceived;
            videoSource.Start();

        }







        private void OnFrameReceived(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {

                bitmap.Lock();
                videoSource.TransformTo(bitmap);
                bitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight));
                bitmap.Unlock();


            });
        }

    }
}