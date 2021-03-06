﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Globalization;

namespace WiresharkApp
{
    public class JsonPacketParser
    {

        public List<Packet> ParseJson(String jsonString)
        {
            List<Packet> packets = new List<Packet>();
            List<JsonPacket> jsonPackets = JsonConvert.DeserializeObject<List<JsonPacket>>(jsonString);

            if(jsonPackets == null)
            {
                return new List<Packet>();
            }

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
                Protocol = ParseProtocol(jsonPacket),
                Length = ParseLength(jsonPacket),
                Time = ParseTime(jsonPacket),
                Source_MAC = ParseSourceMAC(jsonPacket),
                Source_IP = ParseSourceIP(jsonPacket),
                Source_Port = ParseSourcePort(jsonPacket),
                Dest_MAC = ParseDestMAC(jsonPacket),
                Dest_IP = ParseDestIP(jsonPacket),
                Dest_Port = ParseDestPort(jsonPacket)
            };
        }

        private string ParseProtocol(JsonPacket jsonPacket)
        {
            if(jsonPacket._source.layers.tcp != null)
            {
                return "TCP";
            }
            if(jsonPacket._source.layers.udp != null)
            {
                return "UDP";
            }
            if(jsonPacket._source.layers.icmpv6 != null)
            {
                return "ICMPv6";
            }
            if(jsonPacket._source.layers.arp != null)
            {
                return "ARP";
            }
            return "UNKNOWN";
        }


        private string ParseLength(JsonPacket jsonPacket)
        {
            if(jsonPacket?._source?.layers?.tcp != null)
            {
                return jsonPacket._source.layers.tcp.tcp_len;
            }
            if(jsonPacket?._source?.layers?.udp != null)
            {
                return jsonPacket._source.layers.udp.udp_length;
            }
            return "UNKNOWN";
        }

        private DateTime ParseTime(JsonPacket jsonPacket)
        {
            string format = "MMM dd, yyyy HH:mm:ss";
            DateTime dateTime;
            DateTime.TryParseExact(jsonPacket._source.layers.frame.frame_time.Substring(0,21), format, new CultureInfo("en-US"), DateTimeStyles.None, out dateTime);

            return dateTime;
        }

        private string ParseSourceMAC(JsonPacket jsonPacket)
        {
            return jsonPacket?._source?.layers?.eth?.eth_src;
        }

        private string ParseDestMAC(JsonPacket jsonPacket)
        {
            return jsonPacket?._source?.layers?.eth?.eth_dst;
        }

        private string ParseSourceIP(JsonPacket jsonPacket)
        {
            return jsonPacket?._source?.layers?.ip?.ip_src;
        }

        private string ParseDestIP(JsonPacket jsonPacket)
        {
            return jsonPacket?._source?.layers?.ip?.ip_dst;
        }

        private string ParseSourcePort(JsonPacket jsonPacket)
        {
            if(jsonPacket?._source?.layers?.tcp != null)
            {
                return jsonPacket?._source?.layers?.tcp?.tcp_srcport;
            }
            if(jsonPacket?._source?.layers?.udp != null)
            {
                return jsonPacket?._source?.layers?.udp?.udp_srcport;
            }
            return "UNKNOWN";
        }

        private string ParseDestPort(JsonPacket jsonPacket)
        {
            if (jsonPacket?._source?.layers?.tcp != null)
            {
                return jsonPacket?._source?.layers?.tcp?.tcp_dstport;
            }
            if (jsonPacket?._source?.layers?.udp != null)
            {
                return jsonPacket?._source?.layers?.udp?.udp_dstport;
            }
            return "UNKNOWN";
        }
    }
}
