using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
        private int activeBlocks;
        private bool motionTriggered;
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
                    case ViewportMode.CombinedThreshold:
                        gs.MotionDetector.ResultMode = Model.MotionDetection.ResultMode.Combined;
                        break;
                    case ViewportMode.Blocks:
                        gs.MotionDetector.ResultMode = Model.MotionDetection.ResultMode.Blocks;
                        break;
                    case ViewportMode.BlocksCombined:
                        gs.MotionDetector.ResultMode = Model.MotionDetection.ResultMode.BlocksCombined;
                        break;
                    case ViewportMode.Reference:
                        gs.MotionDetector.ResultMode = Model.MotionDetection.ResultMode.Reference;
                        break;

                }
            }
        }

        public string MotionDetectionHotspots
        {
            get => motionDetectionHotspots;
            set => Set(ref motionDetectionHotspots, value);
        }


        public int ActiveBlocks
        {
            get => activeBlocks;
            set => Set(ref activeBlocks, value);
        }
        public bool MotionTriggered
        {
            get => motionTriggered;
            set => Set(ref motionTriggered, value);
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
                    ViewportMode == ViewportMode.Reference ||
                    ViewportMode == ViewportMode.Blocks ||
                    ViewportMode == ViewportMode.BlocksCombined ||
                    ViewportMode == ViewportMode.CombinedThreshold ||
                    viewportMode == ViewportMode.MotionDetectionThreshold)
                {
                    ShowFrame(e.ResultFrame);
                }

                MotionTriggered = e.Motion;
                ActiveBlocks = e.ActiveBlocksCount;
                MotionDetectionHotspotsPercentage = e.HotspotPercentage;
                MotionDetectionHotspots = $"{e.HotspotCount} ({e.HotspotPercentage:0.00}%)";
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


        private System.Windows.Point lastMousePos;
        private float scale = 1f;
        private double xOffset = 0;
        private double yOffset = 0;


        public Thickness ViewerOffset
        {
            get
            {
                return new Thickness(xOffset, yOffset, 0, 0);
            }
        }
        public double ViewerHeight
        {
            get
            {
                if (lastFrame == null)
                {
                    return 1000;
                }
                return lastFrame.Height * scale;
            }
        }
        public double ViewerWidth
        {
            get
            {
                if (lastFrame == null)
                {
                    return 1000;
                }
                return lastFrame.Width * scale;
            }
        }

        /// <summary>
        /// Drag viewport
        /// </summary>
        public void borPreview_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Windows.Point pos = e.GetPosition((Border)sender);

            if (e.LeftButton == MouseButtonState.Pressed ||
                e.RightButton == MouseButtonState.Pressed ||
                e.MiddleButton == MouseButtonState.Pressed)
            {
                Vector diff = pos - lastMousePos;

                xOffset += diff.X;
                yOffset += diff.Y;

            }

            lastMousePos = pos;
            RaisePropertyChanged(nameof(ViewerOffset));

        }


        /// <summary>
        /// Scroll event
        /// </summary>
        public void borPreview_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            const float ZOOM_MULTIPLIER = 1.1f;

            // Calculate cursor position before zoom
            Vector beforePos = new Vector(lastMousePos.X - xOffset, lastMousePos.Y - yOffset);
            beforePos /= scale;

            // Zoom
            if (e.Delta > 0) scale *= ZOOM_MULTIPLIER;
            else scale /= ZOOM_MULTIPLIER;

            beforePos *= scale;

            xOffset = lastMousePos.X - beforePos.X;
            yOffset = lastMousePos.Y - beforePos.Y;
            RaisePropertyChanged(nameof(ViewerOffset));
            RaisePropertyChanged(nameof(ViewerHeight));
            RaisePropertyChanged(nameof(ViewerWidth));
        }



    }
}