using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Configuration;

namespace WiresharkApp
{
    public class DatabaseWriter
    {
        private SQLiteConnection sqliteConnection;

        private Dictionary<string, Device> localDevices;
        private Dictionary<string, Device> remoteDevices;

        public DatabaseWriter()
        {
            sqliteConnection = new SQLiteConnection();
            localDevices = new Dictionary<string, Device>();
            remoteDevices = new Dictionary<string, Device>();
        }

        public void CreateDatabaseFile(string databasePath)
        {
            sqliteConnection.ConnectionString = ConfigurationManager.ConnectionStrings["SQLiteConnectionString"].ConnectionString;
            if (File.Exists(databasePath))
            {
                try
                {
                    File.Delete(databasePath);
                }
				catch (IOException e)
                {
                    Console.WriteLine("Failed to delete file");
                }
            }
            SQLiteConnection.CreateFile(databasePath);
            CreateDatabaseTables();

        }

        public void CreateDatabaseTables()
        {
            SQLiteCommand createTableLocalDevice = new SQLiteCommand();
            SQLiteCommand createTablePacket = new SQLiteCommand();
            SQLiteCommand createTableRemoteDevice = new SQLiteCommand();

            createTableLocalDevice.CommandText = @"CREATE TABLE 'Local_Device' ( 'MAC_Address' VARCHAR PRIMARY KEY, 'IP_Address' VARCHAR, 'Packets' INTEGER, 'Total_Data' INTEGER);";
            createTablePacket.CommandText = @"CREATE TABLE 'Packet' ( 'ID' INTEGER PRIMARY KEY AUTOINCREMENT,'Protocol' VARCHAR(10),'Length' INTEGER(5),'Time' TIMESTAMP,'Source_MAC' VARCHAR(17),'Source_IP' VARCHAR(30),'Source_Port' INTEGER(10),'Dest_MAC' VARCHAR(17),'Dest_IP' VARCHAR(30),'Dest_Port' INTEGER(10));";
            createTableRemoteDevice.CommandText = @"CREATE TABLE 'Remote_Device' ( 'IP_Address' VARCHAR PRIMARY KEY, 'MAC_Address' VARCHAR, 'Packets' INTEGER, 'Total_Data' INTEGER);";

            sqliteConnection.Open();

            createTableLocalDevice.Connection = sqliteConnection;
            createTablePacket.Connection = sqliteConnection;
            createTableRemoteDevice.Connection = sqliteConnection;

            createTableLocalDevice.ExecuteNonQuery();
            createTablePacket.ExecuteNonQuery();
            createTableRemoteDevice.ExecuteNonQuery();

            sqliteConnection.Close();

        }

        public void WritePackets(List<Packet> packets)
        {
            SQLiteCommand insertPacket = new SQLiteCommand();

            sqliteConnection.Open();
            insertPacket.Connection = sqliteConnection;
            foreach (Packet packet in packets)
            {
                insertPacket.CommandText = "INSERT INTO Packet (Protocol, Length, Time, Source_MAC, Source_IP, Source_Port, Dest_MAC, Dest_IP, Dest_Port) VALUES ("
                    + '\''+ packet.Protocol + '\'' + "," 
					+ '\'' + packet.Length + '\'' +  "," 
					+ '\'' +  packet.Time + '\'' +  "," 
					+ '\'' +  packet.Source_MAC + '\'' +  "," 
					+ '\'' +  packet.Source_IP + '\'' +  "," 
					+ '\'' +  packet.Source_Port + '\'' +  "," 
					+ '\'' +  packet.Dest_MAC + '\'' +  "," 
					+ '\'' +  packet.Dest_IP + '\'' + "," 
					+ '\'' + packet.Dest_Port + '\'' +  ");";
                insertPacket.ExecuteNonQuery();
            }
            sqliteConnection.Close();
        }

        public void UpdateDevices(List<Packet> packets)
        {
            Dictionary<string, Device> localDevicesToUpdate = new Dictionary<string, Device>();
            Dictionary<string, Device> remoteDevicesToUpdate = new Dictionary<string, Device>();

            foreach (Packet packet in packets)
            {
                Device source;
                Device destination;

                int packetSize = 0;
                int.TryParse(packet.Length, out packetSize);
                if (packet.Source_IP != null)
                {
                    if (packet.Source_IP.StartsWith("192"))
                    {
                        if (localDevicesToUpdate.ContainsKey(packet?.Source_MAC))
                        {
                            source = localDevicesToUpdate[packet.Source_MAC];
                            localDevicesToUpdate[packet.Source_MAC] = new Device { MAC_Address = source.MAC_Address, IP_Address = source.IP_Address, Packets = source.Packets + 1, TotalData = source.TotalData + packetSize };
                        }
                        else
                        {
                            localDevicesToUpdate.Add(packet.Source_MAC, new Device { MAC_Address = packet.Source_MAC, IP_Address = packet.Source_IP, Packets = 1, TotalData = packetSize });
                        }
                    }
                    else
                    {
                        if (remoteDevicesToUpdate.ContainsKey(packet?.Source_IP))
                        {
                            source = remoteDevicesToUpdate[packet.Source_IP];
                            remoteDevicesToUpdate[packet.Source_IP] = new Device { MAC_Address = source.MAC_Address, IP_Address = source.IP_Address, Packets = source.Packets + 1, TotalData = source.TotalData + packetSize };
                        }
                        else
                        {
                            remoteDevicesToUpdate.Add(packet.Source_IP, new Device { MAC_Address = packet.Source_MAC, IP_Address = packet.Source_IP, Packets = 1, TotalData = packetSize });
                        }
                    }
                }

                if (packet.Dest_IP != null)
                {
                    if (packet.Dest_IP.StartsWith("192"))
                    {
                        if (localDevicesToUpdate.ContainsKey(packet?.Dest_MAC))
                        {
                            destination = localDevicesToUpdate[packet.Dest_MAC];
                            localDevicesToUpdate[packet.Dest_MAC] = new Device { MAC_Address = destination.MAC_Address, IP_Address = destination.IP_Address, Packets = destination.Packets + 1, TotalData = destination.TotalData + packetSize };
                        }
                        else
                        {
                            localDevicesToUpdate.Add(packet.Dest_MAC, new Device { MAC_Address = packet.Source_MAC, IP_Address = packet.Source_IP, Packets = 1, TotalData = packetSize });
                        }
                    }
                    else
                    {
                        if (remoteDevicesToUpdate.ContainsKey(packet?.Dest_IP))
                        {
                            destination = remoteDevicesToUpdate[packet.Dest_IP];
                            remoteDevicesToUpdate[packet.Dest_IP] = new Device { MAC_Address = destination.MAC_Address, IP_Address = destination.IP_Address, Packets = destination.Packets + 1, TotalData = destination.TotalData + packetSize };
                        }
                        else
                        {
                            remoteDevicesToUpdate.Add(packet.Dest_IP, new Device { MAC_Address = packet.Dest_MAC, IP_Address = packet.Dest_IP, Packets = 1, TotalData = packetSize });
                        }
                    }
                }

                //if (/*packet.Source_IP.StartsWith("192") && */localDevicesToUpdate.ContainsKey(packet?.Source_MAC))
                //{
                //    localDevice = localDevicesToUpdate[packet.Source_MAC];
                //    localDevicesToUpdate[packet.Source_MAC] = new Device { MAC_Address = localDevice.MAC_Address, IP_Address = localDevice.IP_Address, Packets = localDevice.Packets + 1, TotalData = localDevice.TotalData + packetSize};
                //}
                //else if(true/* && packet.Source_IP.StartsWith("192")*/)
                //{
                //    localDevicesToUpdate.Add(packet.Source_MAC, new Device { MAC_Address = packet.Source_MAC, IP_Address = packet.Source_IP, Packets = 1, TotalData = packetSize });
                //}

                //if (/*!packet.Dest_IP.StartsWith("192") &&*/ remoteDevicesToUpdate.ContainsKey(packet?.Dest_IP))
                //{
                //    remoteDevice = remoteDevicesToUpdate[packet.Dest_IP];
                //    remoteDevicesToUpdate[packet.Dest_IP] = new Device { MAC_Address = remoteDevice.MAC_Address, IP_Address = remoteDevice.IP_Address, Packets = remoteDevice.Packets + 1, TotalData = remoteDevice.TotalData + packetSize };
                //}
                //else if(true/* && !packet.Dest_IP.StartsWith("192")*/)
                //{
                //    remoteDevicesToUpdate.Add(packet.Dest_IP, new Device { MAC_Address = packet.Dest_MAC, IP_Address = packet.Dest_IP, Packets = 1, TotalData = packetSize });
                //}
            }

            UpdateLocalDevices(localDevicesToUpdate);
            UpdateRemoteDevices(remoteDevicesToUpdate);

            foreach (KeyValuePair<string, Device> kvp in localDevicesToUpdate)
            {
                if (localDevices.ContainsKey(kvp.Key))
                {
                    localDevices[kvp.Key] = new Device { MAC_Address = kvp.Value.MAC_Address, IP_Address = kvp.Value.IP_Address, Packets = localDevices[kvp.Key].Packets + kvp.Value.Packets, TotalData = localDevices[kvp.Key].TotalData + kvp.Value.TotalData };
                }
                else
                {
                    localDevices.Add(kvp.Key, kvp.Value);
                }
            }

            foreach (KeyValuePair<string, Device> kvp in remoteDevicesToUpdate)
            {
                if (remoteDevices.ContainsKey(kvp.Key))
                {
                    remoteDevices[kvp.Key] = new Device { MAC_Address = kvp.Value.MAC_Address, IP_Address = kvp.Value.IP_Address, Packets = remoteDevices[kvp.Key].Packets + kvp.Value.Packets, TotalData = remoteDevices[kvp.Key].TotalData + kvp.Value.TotalData };
                }
                else
                {
                    remoteDevices.Add(kvp.Key, kvp.Value);
                }
            }

        }

        private void UpdateLocalDevices(Dictionary<string, Device> localDevicesToUpdate)
        {
            SQLiteCommand updateLocalDevices = new SQLiteCommand(sqliteConnection);
            sqliteConnection.Open();
            foreach (KeyValuePair<string, Device> kvp in localDevicesToUpdate) {
                if (localDevices.ContainsKey(kvp.Key))
                {
                    updateLocalDevices.CommandText = "UPDATE Local_Device SET Packets = " + (kvp.Value.Packets + localDevices[kvp.Key].Packets) + ", Total_Data = " + (kvp.Value.TotalData + localDevices[kvp.Key].TotalData) + " WHERE MAC_Address = " + '\'' + kvp.Value.MAC_Address + '\''+  ";";
                }
                else
                {
                    updateLocalDevices.CommandText = "INSERT INTO Local_Device (MAC_Address, IP_Address, Packets, Total_Data) VALUES (" + '\'' + kvp.Value.MAC_Address + '\'' + "," +  '\'' + kvp.Value.IP_Address + '\'' + "," + kvp.Value.Packets + "," + kvp.Value.TotalData + ");";
                }
                updateLocalDevices.ExecuteNonQuery();
            }
            sqliteConnection.Close();
        }

        private void UpdateRemoteDevices(Dictionary<string, Device> remoteDevicesToUpdate)
        {
            SQLiteCommand updateRemoteDevices = new SQLiteCommand(sqliteConnection);
            sqliteConnection.Open();
            foreach (KeyValuePair<string, Device> kvp in remoteDevicesToUpdate)
            {
                if (remoteDevices.ContainsKey(kvp.Key))
                {
                    updateRemoteDevices.CommandText = "UPDATE Remote_Device SET Packets = " + (kvp.Value.Packets + remoteDevices[kvp.Key].Packets) + ", Total_Data = " + (kvp.Value.TotalData + remoteDevices[kvp.Key].TotalData) + " WHERE MAC_Address = " + '\'' + kvp.Value.MAC_Address + '\'' + ";";
                }
                else
                {
                    updateRemoteDevices.CommandText = "INSERT INTO Remote_Device (MAC_Address, IP_Address, Packets, Total_Data) VALUES (" + '\'' + kvp.Value.MAC_Address + '\'' + "," + '\'' + kvp.Value.IP_Address + '\'' + "," + kvp.Value.Packets + "," + kvp.Value.TotalData + ");";
                }
                updateRemoteDevices.ExecuteNonQuery();
            }
            sqliteConnection.Close();
        }
    }
}
