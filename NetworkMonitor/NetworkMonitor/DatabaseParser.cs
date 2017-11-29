using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetworkMonitor.PacketInfo;

namespace NetworkMonitor
{
    internal class DatabaseParser
    {
        private static SQLiteConnection sqliteConnection = new SQLiteConnection(ConfigurationManager.ConnectionStrings["SQLiteConnectionString"].ConnectionString);
        /// <summary>
        /// Retrieves latest data from our database and updates clients[] with latest client data.
        /// </summary>
        /// <param name="clients">Info on each client from the database</param>
        /// <param name="packets">Info on each packet to display in live feed</param>
        /// <param name="totalPackets">Total number of packets by all connections</param>
        /// <param name="totalData">Total data in KB by all connections</param>
        /// <param name="mostSeenSourceIP">Most seen source IP</param>
        /// <param name="mostSeenDestinationIP">Most seen destination IP</param>
        /// <param name="percentDownloaded">Percentage of data downloaded vs uploaded</param>
        public static void RefreshDatabase(out ClientInfo[] clients, out PacketInfo[] packets, out uint totalPackets, out double totalData, out string mostSeenSourceIP, out string mostSeenDestinationIP, out double percentDownloaded)
        {
            ///TODO: TEMP POPULATION OF CLIENTS, MAKE THIS ACTUALLY GRAB FROM DATABASE LATER
            //Random rnd = new Random();
            //clients = new ClientInfo[7];
            //for (int i = 0; i < clients.Length; i++)
            //{
            //   uint[] packetSamples = new uint[60];
            //   for (int p = 0; p < packetSamples.Length; p++)
            //       packetSamples[p] = (uint)rnd.Next(0, 4900);

            //   clients[i] = new ClientInfo("Client " + (i + 1), (uint)rnd.Next(), (double)(rnd.NextDouble() * 2000000), "00:00:00:00:00:00", packetSamples);
            //}

            //packets = new PacketInfo[20];
            //for (int i = 0; i < packets.Length; i++)
            //{
            //    packets[i] = new PacketInfo("192.168.255.255", "192.168.255.255", PacketInfo.PacketProtocol.UNKNOWN, 99999, 9999.99); 
            //}

            //totalPackets = 50;
            //totalData = 100000;
            //mostSeenRemoteIP = "200.9.100.39";
            //mostSeenLocalIP = "192.168.1.5";
            packets = GetAllRecentPackets();
            totalPackets = (uint) packets.Length;

            totalData = 0;
            double downloadData = 0;
            Dictionary<String, int> sourceIPs = new Dictionary<string, int>();
            Dictionary<String, int> destinationIPs = new Dictionary<string, int>();
            Dictionary<String, int> MACAddresses = new Dictionary<string, int>();
            foreach (PacketInfo packet in packets)
            {
                double toAdd = 0;
                Double.TryParse(packet.Size, out toAdd);
                totalData += toAdd;
                if (sourceIPs.ContainsKey(packet.SourceAddress))
                {
                    sourceIPs[packet.SourceAddress] += 1;
                }
                else sourceIPs[packet.SourceAddress] = 1;
                if (destinationIPs.ContainsKey(packet.DestAddress)) destinationIPs[packet.DestAddress] += 1; else destinationIPs[packet.DestAddress] = 1;
                if (packet.SourceAddress.StartsWith("192"))
                {
                    if (MACAddresses.ContainsKey(packet.SourceMAC))
                    {
                        MACAddresses[packet.SourceMAC] += 1;
                    }
                    else
                    {
                        MACAddresses.Add(packet.SourceMAC, 1);
                    }
                }
                if (packet.DestAddress.StartsWith("192"))
                {
                    if (MACAddresses.ContainsKey(packet.DestMAC))
                    {
                        MACAddresses[packet.DestMAC] += 1;
                    }
                    else
                    {
                        MACAddresses.Add(packet.DestMAC, 1);
                    }
                    double temp;
                    Double.TryParse(packet.Size, out temp);
                    downloadData += temp;
                }
            }
            if (sourceIPs.Count != 0 && destinationIPs.Count != 0)
            {
                mostSeenSourceIP = sourceIPs.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
                mostSeenDestinationIP = destinationIPs.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            }
            else
            {
                mostSeenSourceIP = "Data not sampled yet";
                mostSeenDestinationIP = "Data not sampled yet";
            }

            DeviceInfo[] localDevices = GetAllLocalDevices();
            localDevices = localDevices.OrderByDescending(x => x.TotalDataUsage).ToArray();

            clients = new ClientInfo[7];
            for(int i = 0; i < clients.Length; i++)
            {
                clients[i] = new ClientInfo("Client " + i, 0, 0, "", new uint[60]);
            }
            for(int i = 0; i < 7 && i < localDevices.Length; i++)
            {
                clients[i] = new ClientInfo(localDevices[i].IPAddress, (uint) localDevices[i].PacketCount, localDevices[i].TotalDataUsage, localDevices[i].MACAddress, new uint[60]);
            }

            for(int i = 0; i < localDevices.Length; i++)
            {
                uint[] packetSamples = new uint[60];
                foreach(PacketInfo packet in packets)
                {
                    if(packet.SourceMAC == localDevices[i].MACAddress || packet.DestMAC == localDevices[i].MACAddress)
                    {
                        packetSamples[(DateTime.Now - packet.Time).Seconds]++;
                    }
                }
                clients[i].packetSamples = packetSamples;
            }
            if (totalData > 0)
            {
                percentDownloaded = (downloadData / totalData) * 100;
            }
            else
            {
                percentDownloaded = 0;
            }
            totalData /= 1000;
        }

        private static PacketInfo[] GetAllPackets()
        {
            List<PacketInfo> packets = new List<PacketInfo>();
            SQLiteCommand readAllPackets = new SQLiteCommand();
            readAllPackets.CommandText = "SELECT * FROM Packet";
            readAllPackets.Connection = sqliteConnection;

            sqliteConnection.Open();
            SQLiteDataReader packetReader = readAllPackets.ExecuteReader();
            while (packetReader.Read())
            {
                NameValueCollection entries = packetReader.GetValues();
                packets.Add(new PacketInfo(entries.Get("Source_MAC"), entries.Get("Dest_MAC"), entries.Get("Source_IP"), entries.Get("Dest_IP"), (PacketProtocol) Enum.Parse(typeof(PacketProtocol), entries.Get("Protocol"), true), entries.Get("Source_Port"),entries.Get("Length"), DateTime.Parse(entries.Get("Time"))));
            }
            sqliteConnection.Close();
            return packets.ToArray();   
        }

        private static PacketInfo[] GetAllRecentPackets()
        {
            List<PacketInfo> packets = new List<PacketInfo>();
            SQLiteCommand readAllPackets = new SQLiteCommand();
            readAllPackets.CommandText = "SELECT * FROM Packet WHERE ('now' - Time) < 60";
            readAllPackets.Connection = sqliteConnection;

            sqliteConnection.Open();
            SQLiteDataReader packetReader = readAllPackets.ExecuteReader();
            while (packetReader.Read())
            {
                NameValueCollection entries = packetReader.GetValues();
                packets.Add(new PacketInfo(entries.Get("Source_MAC"), entries.Get("Dest_MAC"), entries.Get("Source_IP"), entries.Get("Dest_IP"), (PacketProtocol)Enum.Parse(typeof(PacketProtocol), entries.Get("Protocol"), true), entries.Get("Source_Port"), entries.Get("Length"), DateTime.Parse(entries.Get("Time"))));
            }
            sqliteConnection.Close();
            return packets.ToArray();
        }

        private static DeviceInfo[] GetAllLocalDevices()
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();
            SQLiteCommand readAllLocalDevices = new SQLiteCommand();
            readAllLocalDevices.CommandText = "SELECT * FROM Local_Device";
            readAllLocalDevices.Connection = sqliteConnection;

            sqliteConnection.Open();
            SQLiteDataReader localDeviceReader = readAllLocalDevices.ExecuteReader();
            while (localDeviceReader.Read())
            {
                NameValueCollection entries = localDeviceReader.GetValues();
                devices.Add(new DeviceInfo(entries.Get("MAC_Address"), entries.Get("IP_Address"), Int32.Parse(entries.Get("Packets")), Int32.Parse(entries.Get("Total_Data"))));
            }
            sqliteConnection.Close();
            return devices.ToArray();
        }

        private static DeviceInfo[] GetAllRemoteDevices()
        {
            List<DeviceInfo> devices = new List<DeviceInfo>();
            SQLiteCommand readAllLocalDevices = new SQLiteCommand();
            readAllLocalDevices.CommandText = "SELECT * FROM Remote_Device";
            readAllLocalDevices.Connection = sqliteConnection;

            sqliteConnection.Open();
            SQLiteDataReader localDeviceReader = readAllLocalDevices.ExecuteReader();
            while (localDeviceReader.Read())
            {
                NameValueCollection entries = localDeviceReader.GetValues();
                devices.Add(new DeviceInfo(entries.Get("MAC_Address"), entries.Get("IP_Address"), Int32.Parse(entries.Get("Packets")), Int32.Parse(entries.Get("Total_Data"))));
            }
            sqliteConnection.Close();
            return devices.ToArray();
        }

    }
}
