using System;

namespace NetworkMonitor
{
    /// <summary>
    /// PacketInfo.cs - V1
    /// 
    /// Stores info read in from database about a given client.
    /// </summary>
    public struct DeviceInfo
    {

        public DeviceInfo(string mac, string ip, int pc, int totalData)
        {
            MAC = mac;
            ipAddress = ip;
            packetCount = pc;
            totalDataUsage = totalData;
        }

        private string MAC;
        private string ipAddress;
        private int packetCount;
        private int totalDataUsage;

        /// <summary>
        /// MAC address this packe
        /// </summary>
        public string MACAddress { get { return MAC; } }
        /// <summary>
        /// IP address of this device
        /// </summary>
        public string IPAddress { get { return ipAddress; } }
        /// <summary>
        /// Total number of packets sent/received by this device
        /// </summary>
        public int PacketCount { get { return packetCount; } }
        /// <summary>
        /// Total count of data usage for this device (in Kb)
        /// </summary>
        public int TotalDataUsage { get { return totalDataUsage; } }
    }
}
