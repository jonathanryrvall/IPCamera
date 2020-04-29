using System;
using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using GalaSoft.MvvmLight.Command;
using RtspClientSharp;
using SimpleRtspPlayer.RawFramesReceiving;

namespace SimpleRtspPlayer.GUI.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private string _status = string.Empty;
      

        public string DeviceAddress { get; set; } = "rtsp://admin:admins@192.168.1.101/user=admin_password=_channel=1_stream=0.sdp";

        public string Login { get; set; } = "admin";
        public string Password { get; set; } = "123456";

      
        public RelayCommand StartClickCommand { get; }
        public RelayCommand<CancelEventArgs> ClosingCommand { get; }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindowViewModel()
        {
            
            StartClickCommand = new RelayCommand(OnStartButtonClick);
   
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnStartButtonClick()
        {
         
           

            if (!Uri.TryCreate(DeviceAddress, UriKind.Absolute, out Uri deviceUri))
            {
                MessageBox.Show("Invalid device address", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var credential = new NetworkCredential(Login, Password);

            var connectionParameters = !string.IsNullOrEmpty(deviceUri.UserInfo) ? new ConnectionParameters(deviceUri) : 
                new ConnectionParameters(deviceUri, credential);

            connectionParameters.RtpTransport = RtpTransportProtocol.UDP;
            connectionParameters.CancelTimeout = TimeSpan.FromSeconds(1);

            Start(connectionParameters);
            StatusChanged += MainWindowModelOnStatusChanged;

    
        }

      

        private void MainWindowModelOnStatusChanged(object sender, string s)
        {
            Application.Current.Dispatcher.Invoke(() => Status = s);
        }

        













        private readonly RealtimeVideoSource _realtimeVideoSource = new RealtimeVideoSource();

        private RawFramesSource _rawFramesSource;

        public event EventHandler<string> StatusChanged;

        public RealtimeVideoSource VideoSource => _realtimeVideoSource;

        public void Start(ConnectionParameters connectionParameters)
        {
            if (_rawFramesSource != null)
                return;

            _rawFramesSource = new RawFramesSource(connectionParameters);
            _rawFramesSource.ConnectionStatusChanged += ConnectionStatusChanged;

            _realtimeVideoSource.SetRawFramesSource(_rawFramesSource);

            _rawFramesSource.Start();
        }

        public void Stop()
        {
            if (_rawFramesSource == null)
                return;

            _rawFramesSource.Stop();
            _realtimeVideoSource.SetRawFramesSource(null);
            _rawFramesSource = null;
        }

        private void ConnectionStatusChanged(object sender, string s)
        {
            StatusChanged?.Invoke(this, s);
        }
    }
}