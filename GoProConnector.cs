using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;

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
        public bool TurnOffFlag { get; set; }
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
        }
        public void Connect()
        {
            var networkInterface =  this.GetGoProNetworkInterface();
            if(networkInterface != null)
            {
                this.GoProIPAddress = networkInterface.GetIPProperties().DhcpServerAddresses.FirstOrDefault().MapToIPv4();
                this.Connected = true;
                this.Resolution = "1080";
                this.Lens = 6;
                this.GetStatus();
                OnConnect?.Invoke(this, null);
                this.SetNarrowMode();
            }
        }

        public void Stop()
        {
            this.Started = false;  
            this.Send("/gp/gpWebcam/STOP");
        }
        public void Start()
        {
            this.Started = true; 
            this.Send("/gp/gpWebcam/START?res=" + this.Resolution);
        }
        public void PreTurnOff()
        {
            this.Stop();
            this.TurnOffFlag = true;
            System.Threading.Thread.Sleep(2000);
            this.GetStatus();
            this.NotConnected();
        }
        public void TurnOff()
        {
            this.Send("/gp/gpControl/command/system/sleep");
            this.TurnOffFlag = false;
        }

        public void SetWideMode()
        {
            this.Set("/gp/gpControl/setting/43/0");
            this.Lens = 0;
            OnData?.Invoke(this, null);
            this.GetCurrentLens();
        }

        public void SetNarrowMode()
        {
            this.Set("/gp/gpControl/setting/43/6");
            this.Lens = 6;
            OnData?.Invoke(this, null);
            this.GetCurrentLens();
        }

        public void SetLinearMode()
        {
            this.Set("/gp/gpControl/setting/43/4");
            this.Lens = 4;
            OnData?.Invoke(this, null);
            this.GetCurrentLens();
        }     

        public void SetRes720()
        {
            this.Stop();
            this.Resolution = "720";
            OnData?.Invoke(this, null);
            this.Start();
        }     
        public void SetRes1080()
        {
            this.Stop();
            this.Resolution = "1080";
            OnData?.Invoke(this, null);
            this.Start();
        }     
        public void GetCurrentLens()
        {
            
            this.GetStatus();
        }   

        public void GetStatus()
        {
            this.Get("/gp/gpControl/status");
        }   

        private void Set(string url)
        {
            this.Stop();
            this.Send(url);
            this.Start();
        }

        private async void Get(string url)
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
                else if(this.TurnOffFlag) {
                    this.Started = false;
                    this.TurnOff();
                } else 
                    this.Started = false;

                this.Name = goprostatus.status.name;
                this.BatteryPercent = goprostatus.status.batteryPercent;
            }
            OnData?.Invoke(this, null);
        }

        private async void Send(string url)
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
            OnDisconnect?.Invoke(this, null);
            this.Resolution = "";
            this.Name = "";
            this.BatteryPercent = 0;
            OnData?.Invoke(this, null);
        }

        private NetworkInterface GetGoProNetworkInterface()
        {
            string value = string.Empty;
            var goproNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.Description == "GoPro RNDIS Device");
            if(goproNetworkInterface == null)
            {
                this.NotConnected();
            }
            return goproNetworkInterface;
        }
    }
}
