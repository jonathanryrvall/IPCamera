using GalaSoft.MvvmLight;
using IPCamera.Model.Recording;
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
        private double maxHotspots = 20;
        private int preRecord = 32;
        private int recordTime = 5;
        private Bitrate bitrate = Bitrate.K4000;

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

        public double MaxHotspots
        {
            get => maxHotspots;
            set => Set(ref maxHotspots, value);
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
        public void AddMissing()
        {
            
        }
    }
}
