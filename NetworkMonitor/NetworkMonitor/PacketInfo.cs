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
            TCP
        }

        public PacketInfo(string sourceIP, string destIP, PacketProtocol packetProtocol, uint packetPort, double packetSize)
        {
            sourceAddress = sourceIP;
            destAddress = destIP;
            protocol = packetProtocol;
            localPort = packetPort;
            size = packetSize;
        }

        private string sourceAddress;
        private string destAddress;
        private PacketProtocol protocol;
        private uint localPort;
        private double size;

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
        public uint Port { get { return localPort; } }
        /// <summary>
        /// Size of this packet in KB
        /// </summary>
        public double Size { get { return size; } }
    }
}
