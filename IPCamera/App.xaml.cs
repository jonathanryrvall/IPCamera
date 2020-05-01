using IPCamera.Model;
using IPCamera.Model.Config;
using IPCamera.Model.Recording;
using IPCamera.View;
using System;

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
            gs.Config = ConfigSaverLoader.LoadCreateDefault<Config>(FilePaths.ConfigPath());

            // Setup a video source
            gs.VideoSource = new VideoSource(gs.Config);

            // Setup a recorder
            gs.Recorder = new ImageRecorder();
            gs.Recorder.Setup(gs.VideoSource, FilePaths.RecordPath());

            // Setup motion detection
            gs.MotionDetector = new Model.MotionDetection.MotionDetector(gs.VideoSource, gs.Config);


            // Start main window
            application.Run(new MainV());
        }

    }
}