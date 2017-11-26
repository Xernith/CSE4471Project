using Newtonsoft.Json;
using WiresharkApp;

namespace WiresharkApp
{
    public class Frame
    {
        [JsonProperty("frame.time")]
        public string frame_time { get; set; }
    }

    public class Eth
    {
        [JsonProperty("eth.dst")]
        public string eth_dst { get; set; }
        [JsonProperty("eth.src")]
        public string eth_src { get; set; }
    }

    public class Ip
    { 
        [JsonProperty("ip.src")]
        public string ip_src { get; set; }
        [JsonProperty("ip.dst")]
        public string ip_dst { get; set; }
    }

    public class Tcp
    {
        [JsonProperty("tcp.srcport")]
        public string tcp_srcport { get; set; }
        [JsonProperty("tcp.dstport")]
        public string tcp_dstport { get; set; }
        [JsonProperty("tcp.len")]
        public string tcp_len { get; set; }
        [JsonProperty("tcp.payload")]
        public string tcp_payload { get; set; }
    }


    public class Udp
    {
        [JsonProperty("udp.srcport")]
        public string udp_srcport { get; set; }
        [JsonProperty("udp,dstport")]
        public string udp_dstport { get; set; }
        [JsonProperty("udp.length")]
        public string udp_length { get; set; }
    }

    public class Arp
    {

    }

    public class Icmpv6
    {


    }
    public class Layers
    {
        public Frame frame { get; set; }
        public Eth eth { get; set; }
        public Ip ip { get; set; }
        public Tcp tcp { get; set; }
        public Udp udp { get; set; }
        public Icmpv6 icmpv6 { get; set; }
        public Arp arp { get; set; }
    }

    public class Source
    {
        public Layers layers { get; set; }
    }

    public class JsonPacket
    {
        public Source _source { get; set; }
    }
}
