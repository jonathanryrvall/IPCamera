using System;
using System.Collections.Generic;
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
        public RelayCommand SnapshotCommand { get; set; }

        private ViewportMode viewportMode;
        private ImageFrame lastFrame;
        public Config Config => gs.Config;
        private string motionDetectionHotspots;

        private double motionDetectionHotspotsPercentage;

        public WriteableBitmap LiveImage
        {
            get => liveImage;
            set => Set(ref liveImage, value);
        }
        public double MotionDetectionHotspotsPercentage
        {
            get => motionDetectionHotspotsPercentage;
            set => Set(ref motionDetectionHotspotsPercentage, value);
        }
        public Visibility ShowRecordingFrame
        {
            get => gs.Recorder.IsRecording ? Visibility.Visible : Visibility.Hidden;
        }

        public ViewportMode ViewportMode
        {
            get => viewportMode;
            set
            {
                Set(ref viewportMode, value);

                switch (viewportMode)
                {
                    case ViewportMode.MotionDetectionDiff:
                        gs.MotionDetector.ResultMode = Model.MotionDetection.ResultMode.Diff;
                        break;
                    case ViewportMode.MotionDetectionThreshold:
                        gs.MotionDetector.ResultMode = Model.MotionDetection.ResultMode.Threshold;
                        break;

                }
            }
        }

        public string MotionDetectionHotspots
        {
            get => motionDetectionHotspots;
            set => Set(ref motionDetectionHotspots, value);
        }



        /// <summary>
        /// Get a dictionary of <see cref="Model.ViewportMode"/>s
        /// </summary>
        public Dictionary<ViewportMode, string> ViewportModes
        {
            get
            {
                var modes = new Dictionary<ViewportMode, string>();

                foreach (ViewportMode t in (ViewportMode[])Enum.GetValues(typeof(ViewportMode)))
                {
                    string desc = t.ToString();
                    modes.Add(t, desc);
                }
                return modes;
            }
        }

        /// <summary>
        /// Get a dictionary of <see cref="Bitrate"/>s
        /// </summary>
        public Dictionary<Bitrate, string> Bitrates
        {
            get
            {
                var bitrates = new Dictionary<Bitrate, string>();

                foreach (Bitrate t in (Bitrate[])Enum.GetValues(typeof(Bitrate)))
                {
                    string desc = t.ToString();
                    bitrates.Add(t, desc);
                }
                return bitrates;
            }
        }

        /// <summary>
        /// Video source
        /// </summary>
        public MainVM()
        {
            gs.VideoSource.DecodedFrameReceived += OnFrameReceived;
            gs.MotionDetector.OnMotionDetectionResult += MotionDetector_OnMotionDetectionResult;
            StartRecordCommand = new RelayCommand(gs.Recorder.Start);
       //     StopRecordCommand = new RelayCommand(gs.Recorder.Stop);
            SnapshotCommand = new RelayCommand(Snapshot);
        }



        /// <summary>
        /// Recieve result from motion detection
        /// </summary>
        private void MotionDetector_OnMotionDetectionResult(object sender, Model.MotionDetection.MotionDetectionResult e)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                if (ViewportMode == ViewportMode.MotionDetectionDiff ||
                    viewportMode == ViewportMode.MotionDetectionThreshold)
                {
                    ShowFrame(e.ResultFrame);
                }


                MotionDetectionHotspotsPercentage = e.HotspotPercentage;
                MotionDetectionHotspots = $"{e.HotspotCount} ({e.HotspotPercentage:0.0}%)";
            });

        }


        /// <summary>
        /// Recieved frame from viewo source
        /// </summary>
        private void OnFrameReceived(object sender, ImageFrame frame)
        {

            App.Current.Dispatcher.Invoke(() =>
            {
                lastFrame = frame;
                RaisePropertyChanged(nameof(ShowRecordingFrame));

                if (ViewportMode == ViewportMode.Image)
                {
                    ShowFrame(frame);
                }
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

        /// <summary>
        /// Take a snapshot
        /// </summary>
        private void Snapshot()
        {
            FrameSaver.Save(lastFrame, FrameSaver.GetTimestampFilename());
        }


    }
}