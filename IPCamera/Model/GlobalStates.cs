using GalaSoft.MvvmLight;
using IPCamera.Model;
using IPCamera.Model.MotionDetection;
using IPCamera.Model.Recording;

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

        public IRecorder Recorder;
        public MotionDetector MotionDetector;

    }
}
