using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.Config
{
    public class Config : ObservableObject
    {
        private string connectionString = "rtsp://admin:admins@192.168.1.101/user=admin_password=_channel=1_stream=0.sdp";

        private byte hotSpotThreshold = 50;
        private double maxHotSpots = 20;


        public string ConnectionStrinng
        {
            get => connectionString;
            set => Set(ref connectionString, value);
        }

        public byte HotSpotThreshold
        {
            get => hotSpotThreshold;
            set => Set(ref hotSpotThreshold, value);
        }

        public double MaxHotSpots
        {
            get => maxHotSpots;
            set => Set(ref maxHotSpots, value);
        }
    }
}
