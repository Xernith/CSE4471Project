using System;

namespace NetworkMonitor
{
    /// <summary>
    /// PacketInfo.cs - V1
    /// 
    /// Stores info read in from database about a given client.
    /// </summary>
    public struct PacketInfo
    {
        public enum PacketProtocol
        {
            UNKNOWN,
            UDP,
            TCP,
            ARP,
            ICMPV6
        }

        public PacketInfo(string packetSourceMAC, string packetDestMAC, string sourceIP, string destIP, PacketProtocol packetProtocol, string packetPort, string packetSize, DateTime packetTime)
        {
            sourceMAC = packetSourceMAC;
            destMAC = packetDestMAC;
            sourceAddress = sourceIP;
            destAddress = destIP;
            protocol = packetProtocol;
            localPort = packetPort;
            size = packetSize;
            time = packetTime;
        }

        private string sourceMAC;
        private string destMAC;
        private string sourceAddress;
        private string destAddress;
        private PacketProtocol protocol;
        private string localPort;
        private string size;
        private DateTime time;

        /// <summary>
        /// Source MAC address this packet came from
        /// </summary>
        public string SourceMAC { get { return sourceMAC; } }
        /// <summary>
        /// Dest MAC address this packet is going to
        /// </summary>
        public string DestMAC { get { return destMAC; } }
        /// <summary>
        /// Source IP address this packet came from
        /// </summary>
        public string SourceAddress { get { return sourceAddress; } }
        /// <summary>
        /// Destination address this packet is bound to
        /// </summary>
        public string DestAddress { get { return destAddress; } }
        /// <summary>
        /// Protocol that this packet uses
        /// </summary>
        public PacketProtocol Protocol { get { return protocol; } }
        /// <summary>
        /// Local port this packet uses
        /// </summary>
        public string Port { get { return localPort; } }
        /// <summary>
        /// Size of this packet in KB
        /// </summary>
        public string Size { get { return size; } }
        /// <summary>
        /// Time the packet was sent
        /// </summary>
        public DateTime Time { get { return time; } }
    }
}
