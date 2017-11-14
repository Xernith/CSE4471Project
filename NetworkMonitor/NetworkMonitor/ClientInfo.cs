using System;

namespace NetworkMonitor
{
    /// <summary>
    /// ClientInfo.cs - V1
    /// 
    /// Stores info read in from database about a given client.
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
        private uint[] packetSamples;

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
        /// <summary>
        /// Each index represents the number of packets sent in the last minute, for up to 60 minutes/entries.
        /// </summary>
        public uint[] PacketSamples { get { return packetSamples; } }
    }
}
