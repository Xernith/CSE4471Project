using System;

namespace NetworkMonitor
{
    /// <summary>
    /// ClientInfo.cs - V1
    /// 
    /// Stores info read in from database about a given client.
    /// 
    /// Code is by Tim Williams, no external plugins are used for this class.
    /// </summary>
    public struct ClientInfo
    {
        public ClientInfo(string clientName, uint totalPackets, double totalDataOverWire, string clientMacAddress, uint[] packetSampleData)
        {
            name = clientName;
            packetCount = totalPackets;
            totalData = totalDataOverWire;
            macAddress = clientMacAddress;
            packetSamples = packetSampleData;
        }

        private string name;
        private uint packetCount;
        private double totalData;
        private string macAddress;
        public uint[] packetSamples;

        /// <summary>
        /// Human-readable name for this client connection
        /// </summary>
        public String Name { get { return name; } }
        /// <summary>
        /// Total number of packets sent/received by this client connection.
        /// </summary>
        public uint PacketCount { get { return packetCount; } }
        /// <summary>
        /// Total data used by this client in KB
        /// </summary>
        public double TotalData { get { return totalData; } }
        /// <summary>
        /// Mac address for this client connection
        /// </summary>
        public string MacAddress { get { return macAddress; } }
    }
}
