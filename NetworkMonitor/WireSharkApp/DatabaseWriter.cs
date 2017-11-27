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

        public DatabaseWriter()
        {
            sqliteConnection = new SQLiteConnection();
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
            SQLiteCommand createTableDevice = new SQLiteCommand();
            SQLiteCommand createTablePacket = new SQLiteCommand();

            createTableDevice.CommandText = "CREATE TABLE \'Device\' ( \'MAC_Address\' VARCHAR(17) NOT NULL, \'IP_Address\' VARCHAR(30) NOT NULL, \'Device_Name\' VARCHAR(50) NOT NULL, PRIMARY KEY (\'MAC_Address\'));";
            createTablePacket.CommandText = "CREATE TABLE \'Packet\' ( \'ID\' INTEGER PRIMARY KEY AUTOINCREMENT,\'Protocol\' VARCHAR(10),\'Length\' INTEGER(5),\'Data\' VARCHAR(2000),\'Time\' TIMESTAMP,\'Source_MAC\' VARCHAR(17),\'Source_IP\' VARCHAR(30),\'Source_Port\' INTEGER(10),\'Dest_MAC\' VARCHAR(17),\'Dest_IP\' VARCHAR(30),\'Dest_Port\' INTEGER(10));";

            sqliteConnection.Open();

            createTableDevice.Connection = sqliteConnection;
            createTablePacket.Connection = sqliteConnection;

            createTableDevice.ExecuteNonQuery();
            createTablePacket.ExecuteNonQuery();

            sqliteConnection.Close();

        }

        public void WritePackets(List<Packet> packets)
        {
            SQLiteCommand insertPacket = new SQLiteCommand();

            sqliteConnection.Open();
            insertPacket.Connection = sqliteConnection;
            foreach (Packet packet in packets)
            {
                insertPacket.CommandText = "INSERT INTO Packet (Protocol, Length, Data, Time, Source_MAC, Source_IP, Source_Port, Dest_MAC, Dest_IP, Dest_Port) VALUES ("
                    + '\''+ packet.Protocol + '\'' + "," 
					+ '\'' + packet.Length + '\'' +  "," 
					+ '\'' +  packet.Data + '\'' +  "," 
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

        public void WriteDevices(List<Device> devices)
        {
            SQLiteCommand insertDevice = new SQLiteCommand();

            sqliteConnection.Open();
            insertDevice.Connection = sqliteConnection;
            foreach (Device device in devices)
            {
                insertDevice.CommandText = "INSERT INTO Device (MAC_Address, IP_Address, Device_Name) VALUES ("
                    + device.MAC_Address + "," + device.IP_Address + "," + device.Device_Name + ");";
                insertDevice.ExecuteNonQuery();
            }
            sqliteConnection.Close();
        }
    }
}
