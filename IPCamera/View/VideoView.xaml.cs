using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using RtspClientSharp;
using SimpleRtspPlayer.RawFramesDecoding;
using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;
using SimpleRtspPlayer.RawFramesReceiving;

namespace SimpleRtspPlayer.GUI.Views
{
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>
    public partial class VideoView
    {
        public string DeviceAddress { get; set; } = "rtsp://admin:admins@192.168.1.101/user=admin_password=_channel=1_stream=0.sdp";

        public string Login { get; set; } = "admin";
        public string Password { get; set; } = "123456";


        private WriteableBitmap _writeableBitmap;

        private Int32Rect _dirtyRect;

        private RawFramesSource _rawFramesSource;




        public VideoView()
        {
            InitializeComponent();

            _dirtyRect = new Int32Rect(0, 0, 1280, 720);



            _writeableBitmap = new WriteableBitmap(
                1280,
                720,
                96.0,
                96.0,
                PixelFormats.Pbgra32,
                null);

            RenderOptions.SetBitmapScalingMode(_writeableBitmap, BitmapScalingMode.NearestNeighbor);

            _writeableBitmap.Lock();

            try
            {
                _writeableBitmap.AddDirtyRect(_dirtyRect);
            }
            finally
            {
                _writeableBitmap.Unlock();
            }

            VideoImage.Source = _writeableBitmap;

            if (!Uri.TryCreate(DeviceAddress, UriKind.Absolute, out Uri deviceUri))
            {
                MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(Login, Password);

            ConnectionParameters connectionParameters;
            if (string.IsNullOrEmpty(deviceUri.UserInfo))
            {
                connectionParameters = new ConnectionParameters(deviceUri);
            }
            else
            {
                connectionParameters = new ConnectionParameters(deviceUri, credential);

            }
       
            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            _rawFramesSource = new RawFramesSource(connectionParameters);

            _rawFramesSource.DecodedFrameReceived += OnFrameReceived;
            _rawFramesSource.Start();

        }


     
    
    
     

        private void OnFrameReceived(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {

                _writeableBitmap.Lock();

                try
                {
                    _rawFramesSource.TransformTo(_writeableBitmap);

                    _writeableBitmap.AddDirtyRect(_dirtyRect);
                }
                finally
                {
                    _writeableBitmap.Unlock();
                }
            });
        }

      


    }
}