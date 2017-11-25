using Newtonsoft.Json;
using WiresharkApp;

namespace WiresharkApp
{
    public class FrameInterfaceIdTree
        {
            public string frame_interface_name { get; set; }
        }

        public class Frame
        {
            public string frame_interface_id { get; set; }
            public FrameInterfaceIdTree frame_interface_id_tree { get; set; }
            public string frame_encap_type { get; set; }
            public string frame_time { get; set; }
            public string frame_offset_shift { get; set; }
            public string frame_time_epoch { get; set; }
            public string frame_time_delta { get; set; }
            public string frame_time_delta_displayed { get; set; }
            public string frame_time_relative { get; set; }
            public string frame_number { get; set; }
            public string frame_len { get; set; }
            public string frame_cap_len { get; set; }
            public string frame_marked { get; set; }
            public string frame_ignored { get; set; }
            public string frame_protocols { get; set; }
            public string frame_coloring_rule_name { get; set; }
            public string frame_coloring_rule_string { get; set; }
        }

        public class EthDstTree
        {
            public string eth_dst_resolved { get; set; }
            public string eth_addr { get; set; }
            public string eth_addr_resolved { get; set; }
            public string eth_lg { get; set; }
            public string eth_ig { get; set; }
        }

        public class EthSrcTree
        {
            public string eth_src_resolved { get; set; }
            public string eth_addr { get; set; }
            public string eth_addr_resolved { get; set; }
            public string eth_lg { get; set; }
            public string eth_ig { get; set; }
        }

        public class Eth
        {
            [JsonProperty("eth.dst")]
            public string eth_dst { get; set; }
            public EthDstTree eth_dst_tree { get; set; }
            [JsonProperty("eth.src")]
            public string eth_src { get; set; }
            public EthSrcTree eth_src_tree { get; set; }
            public string eth_type { get; set; }
        }

        public class IpDsfieldTree
        {
            public string ip_dsfield_dscp { get; set; }
            public string ip_dsfield_ecn { get; set; }
        }

        public class IpFlagsTree
        {
            public string ip_flags_rb { get; set; }
            public string ip_flags_df { get; set; }
            public string ip_flags_mf { get; set; }
        }

        public class Ip
        {
            public string ip_version { get; set; }
            public string ip_hdr_len { get; set; }
            public string ip_dsfield { get; set; }
            public IpDsfieldTree ip_dsfield_tree { get; set; }
            public string ip_len { get; set; }
            public string ip_id { get; set; }
            public string ip_flags { get; set; }
            public IpFlagsTree ip_flags_tree { get; set; }
            public string ip_frag_offset { get; set; }
            public string ip_ttl { get; set; }
            public string ip_proto { get; set; }
            public string ip_checksum { get; set; }
            public string ip_checksum_status { get; set; }
            [JsonProperty("ip.src")]
            public string ip_src { get; set; }
            public string ip_addr { get; set; }
            public string ip_src_host { get; set; }
            public string ip_host { get; set; }
            [JsonProperty("ip.dst")]
            public string ip_dst { get; set; }
            public string ip_dst_host { get; set; }
    }

    public class TcpFlagsTree
    {
        public string tcp_flags_res { get; set; }
        public string tcp_flags_ns { get; set; }
        public string tcp_flags_cwr { get; set; }
        public string tcp_flags_ecn { get; set; }
        public string tcp_flags_urg { get; set; }
        public string tcp_flags_ack { get; set; }
        public string tcp_flags_push { get; set; }
        public string tcp_flags_reset { get; set; }
        public string tcp_flags_syn { get; set; }
        public string tcp_flags_fin { get; set; }
        public string tcp_flags_str { get; set; }
    }

    public class TcpAnalysis
    {
        public string tcp_analysis_bytes_in_flight { get; set; }
        public string tcp_analysis_push_bytes_sent { get; set; }
    }

    public class Tcp
    {
        [JsonProperty("tcp.srcport")]
        public string tcp_srcport { get; set; }
        [JsonProperty("tcp.dstport")]
        public string tcp_dstport { get; set; }
        public string tcp_port { get; set; }
        public string tcp_stream { get; set; }
        [JsonProperty("tcp.len")]
        public string tcp_len { get; set; }
        public string tcp_seq { get; set; }
        public string tcp_nxtseq { get; set; }
        public string tcp_ack { get; set; }
        public string tcp_hdr_len { get; set; }
        public string tcp_flags { get; set; }
        public TcpFlagsTree tcp_flags_tree { get; set; }
        public string tcp_window_size_value { get; set; }
        public string tcp_window_size { get; set; }
        public string tcp_window_size_scalefactor { get; set; }
        public string tcp_checksum { get; set; }
        public string tcp_checksum_status { get; set; }
        public string tcp_urgent_pointer { get; set; }
        public TcpAnalysis tcp_analysis { get; set; }
        [JsonProperty("tcp.payload")]
        public string tcp_payload { get; set; }
    }


    public class Http
    {
        public string http_host { get; set; }
        public string http_request_line { get; set; }
        public string http_connection { get; set; }
        public string http_user_agent { get; set; }
        public string http_accept { get; set; }
        public string http_accept_encoding { get; set; }
        public string http_accept_language { get; set; }
        public string http_request_full_uri { get; set; }
        public string http_request { get; set; }
        public string http_request_number { get; set; }
        public string http_response_in { get; set; }
    }

    public class Layers
    {
        public Frame frame { get; set; }
        public Eth eth { get; set; }
        public Ip ip { get; set; }
        public Tcp tcp { get; set; }
        public Http http { get; set; }
    }

    public class Source
    {
        public Layers layers { get; set; }
    }

    public class JsonPacket
    {
        public string _index { get; set; }
        public string _type { get; set; }
        public object _score { get; set; }
        public Source _source { get; set; }
    }
}
