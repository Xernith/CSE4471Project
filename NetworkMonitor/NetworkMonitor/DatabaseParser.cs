using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkMonitor
{
    internal class DatabaseParser
    {
        /// <summary>
        /// Retrieves latest data from our database and updates clients[] with latest client data.
        /// </summary>
        /// <param name="clients">Info on each client from the database</param>
        /// <param name="packets">Info on each packet to display in live feed</param>
        /// <param name="totalPackets">Total number of packets by all connections</param>
        /// <param name="totalData">Total data in KB by all connections</param>
        /// <param name="mostSeenRemoteIP">Most seen remote IP</param>
        /// <param name="mostSeenLocalIP">Most seen local device</param>
        public static void RefreshDatabase(out ClientInfo[] clients, out PacketInfo[] packets, out uint totalPackets, out double totalData, out string mostSeenRemoteIP, out string mostSeenLocalIP)
        {
            ///TODO: TEMP POPULATION OF CLIENTS, MAKE THIS ACTUALLY GRAB FROM DATABASE LATER
            Random rnd = new Random();
            clients = new ClientInfo[7];
            for (int i = 0; i < clients.Length; i++)
            {
                uint[] packetSamples = new uint[60];
                for (int p = 0; p < packetSamples.Length; p++)
                    packetSamples[p] = (uint)rnd.Next(0, 4900);

                clients[i] = new ClientInfo("Client " + (i + 1), (uint)rnd.Next(), (double)(rnd.NextDouble() * 2000000), "00:00:00:00:00:00", packetSamples);
            }

            packets = new PacketInfo[20];
            for (int i = 0; i < packets.Length; i++)
            {
                packets[i] = new PacketInfo("192.168.255.255", "192.168.255.255", PacketInfo.PacketProtocol.UNKNOWN, 99999, 9999.99);
            }

            totalPackets = 50;
            totalData = 100000;
            mostSeenRemoteIP = "200.9.100.39";
            mostSeenLocalIP = "192.168.1.5";
        }
    }
}
