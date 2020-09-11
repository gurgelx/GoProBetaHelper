using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading.Tasks;

namespace GoPro_Webcam_Beta_helper.GoProConnector
{


    public class GoProClient
    {
        private static readonly HttpClient client = new HttpClient();

        public RawValues RawValues { get; set; }

        public bool Connected { get; set; }
        public bool Started { get; set; }
        public int Lens { get; set; }
        public int BatteryPercent { get; set; }
        public string Name { get; set; }
        public string Resolution { get; set; }
        public IPAddress GoProIPAddress { get; set; }

        public event EventHandler OnConnect;
        public event EventHandler OnData;
        public event EventHandler OnDisconnect;
        public GoProClient()
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            NetworkChange.NetworkAddressChanged += NetworkChanged;
        }

        public async void Connect()
        {
            var goproIpAddress = GetGoProIpAddress();
            if (goproIpAddress != null)
            {
                init();
                GoProIPAddress = goproIpAddress;
                Connected = true;
                try
                {
                    await GetStatus();
                    await GetRawValues();
                }
                catch (Exception e) {}
                OnConnect?.Invoke(this, null);                
            }
        }

        public async Task Stop()
        {
            Started = false;
            await Send("/gp/gpWebcam/STOP");
        }
        public async void Start()
        {
            Started = true;
            await Send("/gp/gpWebcam/START?res=" + Resolution);
        }

        public async void TurnOff()
        {
            await Stop();
            await Send("/gp/gpControl/command/system/sleep");
            NotConnected();
        }

        public async void SetWideMode()
        {
            await Set("/gp/gpControl/setting/43/0");
            Lens = 0;
            OnData?.Invoke(this, null);
        }

        public async void SetNarrowMode()
        {
            await Set("/gp/gpControl/setting/43/6");
            Lens = 6;
            OnData?.Invoke(this, null);
        }

        public async void SetLinearMode()
        {
            await Set("/gp/gpControl/setting/43/4");
            Lens = 4;
            OnData?.Invoke(this, null);
        }

        public async void SetRes720()
        {
            await Stop();
            Resolution = "720";
            OnData?.Invoke(this, null);
            Start();
        }
        public async void SetRes1080()
        {
            await Stop();
            Resolution = "1080";
            OnData?.Invoke(this, null);
            Start();
        }       

        public async Task<GoProStatus> GetStatus()
        {
            var status = await Get<GoProStatus>("/gp/gpControl/status");
            this.HandleStatusResponse(status);
            OnData?.Invoke(this, null);
            return status;
        }

        public async void UpdateStatus()
        {
            var status = await GetStatus();
        }

        public async void SetValue(string setting, string value)
        {
            var url = String.Format("/gp/gpControl/setting/{0}/{1}", setting, value);
            await this.Set(url);
            await this.GetStatus();
            await this.GetRawValues();
        }

        private async Task GetRawValues()
        {
            this.RawValues = await Get<RawValues>("/gp/gpControl/status");
        }

        private void init()
        {
            this.Resolution = "1080";
        }

        private async Task Set(string url)
        {
            await Stop();
            await Send(url);
            Start();
        }

        private async Task<T> Get<T>(string url)
        {
            try
            {
                if (GoProIPAddress != null)
                {
                    var response = await client.GetAsync("http://" + GoProIPAddress + url);
                    var rawJson = response.Content.ReadAsStringAsync().Result;
                    Connected = true;
                    return JsonSerializer.Deserialize<T>(rawJson);                    
                }
            }
            catch (OperationCanceledException e)
            {
                NotConnected();
            }
            return default(T);
        }

        private void HandleStatusResponse(GoProStatus goprostatus)
        {
            if (goprostatus.settings != null)
            {
                Lens = goprostatus.settings.lens;
                switch(goprostatus.settings.resolution)
                {
                    case 4:
                        Resolution = "480";
                        break;
                    case 7:
                        Resolution = "720";
                        break;
                    case 12:
                        Resolution = "1080";
                        break;
                }   
            }
            if (goprostatus.status != null)
            {
                if (goprostatus.status.streaming == 1)
                    Started = true;
                else
                    Started = false;

                Name = goprostatus.status.name;
                BatteryPercent = goprostatus.status.batteryPercent;
            }
        }       

        private async Task Send(string url)
        {
            try
            {
                if (GoProIPAddress != null)
                {
                    await client.GetAsync("http://" + GoProIPAddress + url);
                    Connected = true;
                }
            }
            catch (OperationCanceledException e)
            {
                NotConnected();
            }
            OnData?.Invoke(this, null);
        }

        private void NotConnected()
        {
            Connected = false;
            Resolution = "";
            Name = "";
            BatteryPercent = 0;
            GoProIPAddress = null;
            OnDisconnect?.Invoke(this, null);
            OnData?.Invoke(this, null);
        }

        private IPAddress GetGoProIpAddress()
        {
            string value = string.Empty;
            var goproNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.Description == "GoPro RNDIS Device");
            if (goproNetworkInterface == null)
            {
                NotConnected();
            }
            return goproNetworkInterface?.GetIPProperties().DhcpServerAddresses.FirstOrDefault()?.MapToIPv4();
        }

        private void NetworkChanged(object sender, EventArgs e)
        {
            var goproIpAddress = GetGoProIpAddress();
            if (goproIpAddress != null && !Connected)
            {
                Connect();
            }
            else if (Connected)
            {
                NotConnected();
            }
        }
    }
}
