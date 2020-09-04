using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;

namespace GoPro_Webcam_Beta_helper
{
    public class GoProConnector
    {
        private static readonly HttpClient client = new HttpClient();

        public bool Connected { get; set; }
        public IPAddress GoProIPAddress { get; set; }

        public event EventHandler OnConnect;
        public event EventHandler OnDisconnect;
        public GoProConnector()
        {

        }
        public void Connect()
        {
            var networkInterface =  this.GetGoProNetworkInterface();
            this.GoProIPAddress = networkInterface.GetIPProperties().DhcpServerAddresses.FirstOrDefault().MapToIPv4();
            this.Connected = true;
            OnConnect?.Invoke(this, null);
            this.SetNarrowMode();
        }

        public void Stop()
        {
            this.Send("/gp/gpWebcam/STOP");            
        }
        public void Start()
        {
            this.Send("/gp/gpWebcam/START");
        }

        public void SetWideMode()
        {
            this.Set("/gp/gpControl/setting/43/0");
        }

        public void SetNarrowMode()
        {
            this.Set("/gp/gpControl/setting/43/6");
        }

        public void SetLinearMode()
        {
            this.Set("/gp/gpControl/setting/43/4");
        }       

        private void Set(string url)
        {
            this.Stop();
            this.Send(url);
            this.Start();
        }

        private async void Send(string url)
        {
            try
            {
                await client.GetAsync("http://" + this.GoProIPAddress + url);
            }
            catch(HttpRequestException e)
            {
                
            }
        }
        

        private NetworkInterface GetGoProNetworkInterface()
        {
            string value = string.Empty;
            var goproNetworkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(n => n.Description == "GoPro RNDIS Device");
            if(goproNetworkInterface == null)
            {
                throw new Exception("No GoPro device found");
            }
            return goproNetworkInterface;
        }
    }
}
