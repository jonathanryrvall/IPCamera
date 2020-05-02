using GalaSoft.MvvmLight;
using IPCamera.Model.Recording;
using IPCamera.Model.Scheduling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.Config
{
    public class Config : ObservableObject, IConfig
    {
        private string connectionString = "rtsp://admin:admins@192.168.1.101/user=admin_password=_channel=1_stream=0.sdp";

        private byte hotspotThreshold = 60;
        private int preRecord = 32;
        private int recordTime = 5;
        private Bitrate bitrate = Bitrate.K4000;
        private byte remodelStrength = 10;
        private int remodelInterval = 500;
        private int frameRate = 8;
        private int blockSize = 40;
        private int blockThreshold = 100;
        private int minActiveBlocks = 2;

        private Schedule schedule = new Schedule()
        {
            InactivePeriods = new List<InactivePeriod>()
            {
                new InactivePeriod()
                {
                    StartHour = 8,
                    EndHour = 19
                }
            }
        };

        public int BlockSize
        {
            get => blockSize;
            set => Set(ref blockSize, value);
        }
        public int BlockThreshold
        {
            get => blockThreshold;
            set => Set(ref blockThreshold, value);
        }
        public int MinActiveBlocks
        {
            get => minActiveBlocks;
            set => Set(ref minActiveBlocks, value);
        }


        public string ConnectionString
        {
            get => connectionString;
            set => Set(ref connectionString, value);
        }

        public byte HotspotThreshold
        {
            get => hotspotThreshold;
            set => Set(ref hotspotThreshold, value);
        }
        public int RemodelInterval
        {
            get => remodelInterval;
            set => Set(ref remodelInterval, value);
        }

        public int FrameRate
        {
            get => frameRate;
            set => Set(ref frameRate, value);
        }


        public byte RemodelStrength
        {
            get => remodelStrength;
            set => Set(ref remodelStrength, value);
        }


        public Bitrate Bitrate
        {
            get => bitrate;
            set => Set(ref bitrate, value);
        }
        public int PreRecord
        {
            get => preRecord;
            set => Set(ref preRecord, value);
        }
        public int RecordTime
        {
            get => recordTime;
            set => Set(ref recordTime, value);
        }
        public Schedule Schedule
        {
            get => schedule;
            set => Set(ref schedule, value);
        }

        public void AddMissing()
        {

        }
    }
}
