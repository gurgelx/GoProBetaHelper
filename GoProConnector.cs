using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GoPro_Webcam_Beta_helper
{
    public class Status    {
        [JsonPropertyName("8")]
        public int streaming { get; set; } 
        [JsonPropertyName("30")]
        public string name { get; set; } 
        [JsonPropertyName("70")]
        public int batteryPercent { get; set; } 
    }

    public class Settings    {
        [JsonPropertyName("43")]
        public int lens { get; set; } 
        [JsonPropertyName("47")]
        public int resolution { get; set; } 
    }
    
    public class GoProStatus
    {
        public Status status { get; set; } 
        public Settings settings { get; set; }
    }

    public class GoProConnector
    {
        private static readonly HttpClient client = new HttpClient();

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
        public GoProConnector()
        {
            client.Timeout = TimeSpan.FromSeconds(10);
            NetworkChange.NetworkAddressChanged += NetworkChanged;
        }

        public async void Connect()
        {
            var goproIpAddress =  this.GetGoProIpAddress();
            if(goproIpAddress != null)
            {
                this.GoProIPAddress = goproIpAddress; 
                this.Connected = true;                
                await this.GetStatus();
                OnConnect?.Invoke(this, null);
            }
        }
        
        public async Task Stop()
        {
            this.Started = false;  
            await this.Send("/gp/gpWebcam/STOP");
        }
        public async void Start()
        {
            this.Started = true; 
            await this.Send("/gp/gpWebcam/START?res=" + this.Resolution);
        }
        
        public async void TurnOff()
        {
            await this.Stop();
            await this.Send("/gp/gpControl/command/system/sleep");
            this.NotConnected();
        }

        public void SetWideMode()
        {
            this.Set("/gp/gpControl/setting/43/0");
            this.Lens = 0;
            OnData?.Invoke(this, null);
        }

        public void SetNarrowMode()
        {
            this.Set("/gp/gpControl/setting/43/6");
            this.Lens = 6;
            OnData?.Invoke(this, null);
        }

        public void SetLinearMode()
        {
            this.Set("/gp/gpControl/setting/43/4");
            this.Lens = 4;
            OnData?.Invoke(this, null);
        }     

        public async void SetRes720()
        {
            await this.Stop();
            this.Resolution = "720";
            OnData?.Invoke(this, null);
            this.Start();
        }     
        public async void SetRes1080()
        {
            await this.Stop();
            this.Resolution = "1080";
            OnData?.Invoke(this, null);
            this.Start();
        }     

        public async Task<GoProStatus> GetStatus()
        {
            return await this.Get("/gp/gpControl/status");
        }

        public async void UpdateStatus()
        {
            await this.Get("/gp/gpControl/status");
        }

        private async void Set(string url)
        {
            await this.Stop();
            await this.Send(url);
            this.Start();
        }

        private async Task<GoProStatus> Get(string url)
        {
            var result = "{\"error\":true}";
            var response = new HttpResponseMessage();
            GoProStatus goprostatus = new GoProStatus();
            try
            {
                if(this.GoProIPAddress != null) {
                    response = await client.GetAsync("http://" + this.GoProIPAddress + url);
                    this.Connected = true;
                }
            }
            catch(OperationCanceledException e)
            {
                this.NotConnected();
            }
            if(this.Connected)
            {
                result = response.Content.ReadAsStringAsync().Result;
                goprostatus = JsonSerializer.Deserialize<GoProStatus>(result);
            }

            if(goprostatus.settings != null)
            {
                this.Lens = goprostatus.settings.lens;
                if(goprostatus.settings.resolution == 7)
                    this.Resolution = "720";
                else if(goprostatus.settings.resolution == 12)
                    this.Resolution = "1080";
            }
            if(goprostatus.status != null)
            {
                if(goprostatus.status.streaming == 1)
                    this.Started = true;                
                else 
                    this.Started = false;

                this.Name = goprostatus.status.name;
                this.BatteryPercent = goprostatus.status.batteryPercent;
            }
            OnData?.Invoke(this, null);
            return goprostatus;
        }

        private async Task Send(string url)
        {
            try
            {
                if(this.GoProIPAddress != null) {
                    await client.GetAsync("http://" + this.GoProIPAddress + url);
                    this.Connected = true;
                }
            }
            catch(OperationCanceledException e)
            {
                this.NotConnected();
            }
            OnData?.Invoke(this, null);
        }
        
        private void NotConnected()
        {
            this.Connected = false;            
            this.Resolution = "";
            this.Name = "";
            this.BatteryPercent = 0;
            this.GoProIPAddress = null;
            OnDisconnect?.Invoke(this, null);
            OnData?.Invoke(this, null);
        }

        private IPAddress GetGoProIpAddress()
        {
            string value = string.Empty;
            var goproNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.Description == "GoPro RNDIS Device");
            if(goproNetworkInterface == null)
            {
                this.NotConnected();
            }
            return goproNetworkInterface?.GetIPProperties().DhcpServerAddresses.FirstOrDefault()?.MapToIPv4();
        }

        private void NetworkChanged(object sender, EventArgs e)
        {
            var goproIpAddress = this.GetGoProIpAddress();
            if (goproIpAddress != null && !this.Connected) {
                this.Connect();
            }else if (this.Connected)
            {
                this.NotConnected();
            }
        }
    }
}
