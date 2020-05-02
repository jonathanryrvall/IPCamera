using GalaSoft.MvvmLight;
using IPCamera.Model;
using IPCamera.Model.MotionDetection;
using IPCamera.Model.Recording;
using System;

namespace IPCamera.Model
{
    public class GlobalStates : ObservableObject
    {

        #region Singleton
        private static readonly GlobalStates instance = new GlobalStates();
        static GlobalStates()
        {
        }

        private GlobalStates()
        {

        }

        public static GlobalStates Instance
        {
            get
            {
                return instance;
            }
        }
        #endregion

        public VideoSource VideoSource;
        public Config.Config Config;

        public Recorder Recorder;
        public MotionDetector MotionDetector;

        public Config.ConfigMonitor ConfigMonitor;

        public void ConfigMonitor_ConfigChanged(object sender, EventArgs e)
        {
            Model.Config.ConfigSaverLoader.DelayedSave(Config, FilePaths.ConfigPath());

            MotionDetector.UpdateConfig(Config);
        }

        public void MotionDetector_OnMotionDetectionResult(object sender, Model.MotionDetection.MotionDetectionResult e)
        {
            if (e.Motion)
            {
                Recorder.Start();
            }
        }
    }
}
