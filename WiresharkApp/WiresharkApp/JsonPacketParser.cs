using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WiresharkApp
{
    public class JsonPacketParser
    {

        public List<Packet> ParseJson(String jsonString)
        {
            List<Packet> packets = new List<Packet>();
            List<JsonPacket> jsonPackets = JsonConvert.DeserializeObject<List<JsonPacket>>(jsonString);
            foreach(JsonPacket jsonPacket in jsonPackets)
            {
                packets.Add(ConvertJsonPacket(jsonPacket));
            }
            return packets;
        }

        public Packet ConvertJsonPacket(JsonPacket jsonPacket)
        {
            return new Packet
            {
                Protocol = jsonPacket?._source?.layers?.tcp != null ? "TCP" : "null",
                Length = jsonPacket?._source?.layers?.tcp?.tcp_len != null ? jsonPacket?._source?.layers?.tcp?.tcp_len : "null",
                Data = jsonPacket?._source?.layers?.tcp?.tcp_payload != null ? jsonPacket?._source?.layers?.tcp?.tcp_payload : "null",
                Time = jsonPacket?._source?.layers?.frame?.frame_time != null ? DateTime.Parse(jsonPacket._source.layers.frame.frame_time) : DateTime.Now,
                Source_MAC = jsonPacket?._source?.layers?.eth?.eth_src != null ? jsonPacket?._source?.layers?.eth?.eth_src : "null",
                Source_IP = jsonPacket?._source?.layers?.ip?.ip_src != null ? jsonPacket?._source?.layers?.ip?.ip_src : "null",
                Source_Port = jsonPacket?._source?.layers?.tcp?.tcp_srcport != null ? jsonPacket?._source?.layers?.tcp?.tcp_srcport : "null",
                Dest_MAC = jsonPacket?._source?.layers?.eth?.eth_dst != null ? jsonPacket?._source?.layers?.eth?.eth_dst : "null",
                Dest_IP = jsonPacket?._source?.layers?.ip?.ip_dst != null ? jsonPacket?._source?.layers?.ip?.ip_dst : "null",
                Dest_Port = jsonPacket?._source?.layers?.tcp?.tcp_dstport != null ? jsonPacket?._source?.layers?.tcp?.tcp_dstport : "null"
            };
        }
    }
}
