using IPCamera.Model;
using IPCamera.Model.Config;
using IPCamera.Model.Recording;
using IPCamera.View;
using System;
using System.IO;

namespace IPCamera
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static GlobalStates gs = GlobalStates.Instance;

        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();

            // Read config
            gs.Config = ConfigSaverLoader.Load<Config>(FilePaths.ConfigPath());
            ConfigSaverLoader.Save(gs.Config, FilePaths.ConfigPath());
            gs.ConfigMonitor = new ConfigMonitor(gs.Config);
            gs.ConfigMonitor.ConfigChanged += gs.ConfigMonitor_ConfigChanged;

            // Setup a video source
            gs.VideoSource = new VideoSource(gs.Config);

            // Setup a recorder
            gs.Recorder = new Recorder(gs.VideoSource, 
                                       gs.Config.FrameRate,
                                       gs.Config.PreRecord,
                                       gs.Config.RecordTime,
                                       gs.Config.Bitrate);

            // Setup motion detection
            gs.MotionDetector = new Model.MotionDetection.MotionDetector(gs.VideoSource, gs.Config);
            gs.MotionDetector.OnMotionDetectionResult += gs.MotionDetector_OnMotionDetectionResult;

            // Setup a logger
            gs.Logger = new Logger();
            gs.Logger.NewLog("Startup!");

            // Setup a timelapser
            gs.Timelapser = new Timelapser(gs.VideoSource, gs.Config);

            // Start main window
            application.Run(new MainV());
        }


    }
}