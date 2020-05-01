using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using IPCamera.Model;
using IPCamera.Model.Config;
using IPCamera.Model.Recording;
using RtspClientSharp;


namespace IPCamera.ViewModel
{
    public class MainVM : ViewModelBase
    {
        private GlobalStates gs = GlobalStates.Instance;

        private WriteableBitmap liveImage;
        public RelayCommand StartRecordCommand { get; set; }
        public RelayCommand StopRecordCommand { get; set; }

        public WriteableBitmap LiveImage
        {
            get => liveImage;
            set => Set(ref liveImage, value);
        }

    
        /// <summary>
        /// Video source
        /// </summary>
        public MainVM()
        {
          //  gs.VideoSource.DecodedFrameReceived += OnFrameReceived;
            gs.MotionDetector.OnMotionDetectionResult += MotionDetector_OnMotionDetectionResult;
            StartRecordCommand = new RelayCommand(gs.Recorder.Start);
            StopRecordCommand = new RelayCommand(gs.Recorder.Stop);

        }

        private void MotionDetector_OnMotionDetectionResult(object sender, Model.MotionDetection.MotionDetectionResult e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ShowFrame(e.Bitmap);
            });
        }


        /// <summary>
        /// Recieved frame from viewo source
        /// </summary>
        private void OnFrameReceived(object sender, ImageFrame frame)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                ShowFrame(frame);
            });
        }

        /// <summary>
        /// Show a frame
        /// </summary>
        private void ShowFrame(ImageFrame frame)
        {
            if (LiveImage == null)
            {
                LiveImage = new WriteableBitmap(frame.Width, frame.Height, 96, 96, PixelFormats.Bgr32, null);
            }

            LiveImage.WritePixels(new Int32Rect(0, 0, frame.Width, frame.Height), frame.Data, frame.Width * 4, 0);
        }

    }
}