using GalaSoft.MvvmLight;
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

        public void AddMissing()
        {
            
        }
    }
}
