using System;

namespace WiresharkApp
{
    public class Packet
    {
        public string Protocol { get; set; }
        public string Length { get; set; }
        public DateTime Time { get; set; }
        public string Source_MAC { get; set; }
        public string Source_IP { get; set; }
        public string Source_Port { get; set; }
        public string Dest_MAC { get; set; }
        public string Dest_IP { get; set; }
        public string Dest_Port { get; set; }
    }
}