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
            if (!File.Exists(FilePaths.ConfigPath()))
            {
                ConfigSaverLoader.Save(gs.Config, FilePaths.ConfigPath());
            }
            gs.ConfigMonitor = new ConfigMonitor(gs.Config);
            gs.ConfigMonitor.ConfigChanged += gs.ConfigMonitor_ConfigChanged;

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