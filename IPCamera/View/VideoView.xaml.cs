using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SimpleRtspPlayer.RawFramesDecoding;
using SimpleRtspPlayer.RawFramesDecoding.DecodedFrames;
using SimpleRtspPlayer.RawFramesDecoding.FFmpeg;


namespace SimpleRtspPlayer.GUI.Views
{
    /// <summary>
    /// Interaction logic for VideoView.xaml
    /// </summary>
    public partial class VideoView
    {
        private WriteableBitmap _writeableBitmap;

        private Int32Rect _dirtyRect;
        private TransformParameters _transformParameters;


        public static readonly DependencyProperty VideoSourceProperty = DependencyProperty.Register(nameof(VideoSource),
            typeof(RealtimeVideoSource),
            typeof(VideoView),
            new FrameworkPropertyMetadata(OnVideoSourceChanged));



        public RealtimeVideoSource VideoSource
        {
            get => (RealtimeVideoSource)GetValue(VideoSourceProperty);
            set => SetValue(VideoSourceProperty, value);
        }



        public VideoView()
        {
            InitializeComponent();
            ReinitializeBitmap();
        }


        private void ReinitializeBitmap()
        {
            _dirtyRect = new Int32Rect(0, 0, 1280, 720);

            _transformParameters = new TransformParameters(
                  
                    FFmpegPixelFormat.BGRA, FFmpegScalingQuality.FastBilinear);

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
        }

        private static void OnVideoSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var view = (VideoView)d;

            if (e.OldValue is RealtimeVideoSource oldVideoSource)
                oldVideoSource.FrameReceived -= view.OnFrameReceived;

            if (e.NewValue is RealtimeVideoSource newVideoSource)
                newVideoSource.FrameReceived += view.OnFrameReceived;
        }

        private void OnFrameReceived(object sender, DecodedVideoFrame decodedFrame)
        {
            App.Current.Dispatcher.Invoke(() =>
            {

                Invalidate(decodedFrame);
            });
        }

        private void Invalidate(DecodedVideoFrame decodedVideoFrame)
        {
            _writeableBitmap.Lock();

            try
            {
                decodedVideoFrame.TransformTo(_writeableBitmap.BackBuffer, _writeableBitmap.BackBufferStride, _transformParameters);

                _writeableBitmap.AddDirtyRect(_dirtyRect);
            }
            finally
            {
                _writeableBitmap.Unlock();
            }
        }


    }
}