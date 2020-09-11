using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace GoPro_Webcam_Beta_helper.GoProConnector
{
    public class Status
    {
        [JsonPropertyName("8")]
        public int streaming { get; set; }
        [JsonPropertyName("30")]
        public string name { get; set; }
        [JsonPropertyName("70")]
        public int batteryPercent { get; set; }
    }

    public class Settings
    {
        [JsonPropertyName("43")]
        public int lens { get; set; }
        [JsonPropertyName("64")]
        public int resolution { get; set; }
    }

    public class GoProStatus
    {
        public Status status { get; set; }        
        public Settings settings { get; set; }        
    }

    public class RawValues
    {
        public Dictionary<string, object> settings { get; set; }
        public Dictionary<string, object> status { get; set; }
    }
}
