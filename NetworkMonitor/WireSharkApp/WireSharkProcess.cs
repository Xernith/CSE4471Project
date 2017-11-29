using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WiresharkApp
{
    //WiresharkProcess handles the reading of live network packets into JSON format, calls the DatabaseWriter object to push the packet data to the database
    //Manages everything with intent to minimize as much packet data loss as possible

    public class WiresharkProcess
    {
        public void StartWireShark()
        {

            //set the standard file path for tshark
            String tsfilepath = @"""C:\Program Files\Wireshark\tshark.exe""";

            //check if wireshark is in expected location
            if (!File.Exists(@"C:\Program Files\Wireshark\tshark.exe"))
            {
                Console.Out.WriteLine(@"Tshark not in filepath C:\Program Files\Wireshark\tshark.exe. Please enter expected filepath.");
                tsfilepath = @"""" + Console.ReadLine() + @"""";
                Console.Out.WriteLine(tsfilepath);
            }

            DatabaseWriter databaseWriter = new DatabaseWriter();
            databaseWriter.CreateDatabaseFile("database.sqlite");
            
            //create a buffer ring of files
            //switch files every 5 seconds
            //run constant process that looks for most recent file; if not yet seen, copy it to new file and pass into new thread that processes json into database
            //if file already seen, then wait for half second before checking again*/

            // ******* EVERYTHING FROM VERSION WITH MULTIPROCESSING *******

            string lastFile = "";
            Directory.CreateDirectory("PCAPOutput");
            Directory.CreateDirectory("JSONOutput");
            Process tsProcess = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            tsProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;
            tsProcess.StartInfo.FileName = @"C:\Program Files\Wireshark\tshark.exe";
            tsProcess.StartInfo.Arguments = @"-i 1 -b duration:5 -b files:5 -w PCAPOutput\output.pcap";
            tsProcess.Start();
            //Console.Out.WriteLine(process.StandardOutput.ReadToEnd());

            var directory = new DirectoryInfo("PCAPOutput");
            string tempFileName = "";
            while (true)

            {
                //get most recently written file
                //got this code block from https://stackoverflow.com/questions/1179970/how-to-find-the-most-recent-file-in-a-directory-using-net-and-without-looping
                try
                {
                    var currentFile = (from file in directory.GetFiles() orderby file.LastWriteTime descending select file).ElementAt(2);
                    tempFileName = currentFile.Name;
                }
                catch (ArgumentOutOfRangeException)
                {
                    Thread.Sleep(5000);
                    continue;
                }
                //end of credited code block

                //process tempFile if filename is not the last one seen (linear); otherwise, sleep for a bit
                if (lastFile.Equals(tempFileName))
                {
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    File.Copy(@"PCAPOutput\" + tempFileName, "curJSON.pcap", true);
                    try
                    {   
                        //Open new Tshark process to write to JSON file from curJSON.pcap file

                        Process process1 = new Process();
                        //ProcessStartInfo startInfo1 = new ProcessStartInfo();
                        process1.StartInfo.CreateNoWindow = false;
                        process1.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        process1.StartInfo.RedirectStandardOutput = false;
                        //process1.StartInfo.UseShellExecute = false;

                        process1.StartInfo.FileName = @"cmd.exe";
                        //process1.StartInfo.Arguments = @"/K " + tsfilepath + @"-r curJSON.pcap -T json > JSONOutput\output.json";
                        process1.StartInfo.Arguments = @"/C " + tsfilepath + @" -r curJSON.pcap -T json > JSONOutput\output.json";
                        process1.Start();
                        process1.WaitForExit();
                        process1.Dispose();

                        //Process JSON output into database
                        JsonPacketParser jsonPacketParser = new JsonPacketParser();

                        //Throwing Error here due to usage of output.json by Dumpcap.exe process after running once.  File already in use.
                        List<Packet> packets = jsonPacketParser.ParseJson(File.ReadAllText(@"JSONOutput\output.json"));
                        databaseWriter.UpdateDevices(packets);
                        databaseWriter.WritePackets(packets);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("The file could not be read:");
                        Console.WriteLine(e.Message);
                    }
                }

                //Set lastFile so we know not to read this one again
                lastFile = tempFileName;
            }
            //"C:\Program Files\Wireshark\tshark.exe" -i 2 -a duration:5 -w output.pcap
            //"C:\Program Files\Wireshark\tshark.exe" -r output.pcap -T json > output.json
            //C:\Users\Patrick\Source\Repos\CSE4471Project\WiresharkApp\WiresharkApp\Output\output.pcap
            //Directory.Delete("Output", true);

            // ******* EVERYTHING FROM SIMPLE WIRESHARK TO JSON TO DATABASE PROGRAM

            /*while (true)
            {
                //begin a process for tshark to run in a separate command terminal
                Process process = new Process();
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.FileName = @"cmd.exe";

                //tshark captures 100 packets and outputs the data to a json file
                process.StartInfo.Arguments = @"/C " + tsfilepath + " -i 3 -c 100 -T json > output.json";

                //process begins and program waits until the process ends (i.e. 100 packets have been captured and data output to JSON)
                process.Start();
                process.WaitForExit();
                process.Dispose();

                //TODO: process JSON output into database
                JsonPacketParser jsonPacketParser = new JsonPacketParser();

                //Throwing Error here due to usage of output.json by Dumpcap.exe process after running once.  File already in use.
                List<Packet> packets = jsonPacketParser.ParseJson(File.ReadAllText("output.json"));

                databaseWriter.WritePackets(packets);
            }*/
        }
    }
}
