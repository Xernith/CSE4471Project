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
    class Program
    {
        static void Main(string[] args)
        {

            // ******* EVERYTHING FROM SIMPLE WIRSEHARK TO JSON TO DATABASE PROGRAM

            //set the standard file path for tshark
            String tsfilepath = @"""C:\Program Files\Wireshark\tshark.exe""";

            //check if wireshark is in expected location
            if (!File.Exists(@"C:\Program Files\Wireshark\tshark.exer"))
            {
                Console.Out.WriteLine(@"Tshark not in filepath C:\Program Files\Wireshark\tshark.exe. Please enter expected filepath.");
                tsfilepath = @"""" + Console.ReadLine() + @"""";
                Console.Out.WriteLine(tsfilepath);
            }

            while (true)
            {
                //begin a process for tshark to run in a separate command terminal
                Process process = new Process();
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.StartInfo.FileName = @"cmd.exe";

                //tshark captures 100 packets and outputs the data to a json file
                process.StartInfo.Arguments = @"/K " + tsfilepath + " -i 2 -c 100 -T json > output.json";

                //process begins and program waits until the process ends (i.e. 100 packets have been captured and data output to JSON)
                process.Start();
                process.WaitForExit();

                //TODO: process JSON output into database
            }
            
            //Console.Out.WriteLine(process.StandardOutput.ReadToEnd());

            // ******* EVERYTHING FROM VERSION WITH MULTIPROCESSING *******
            /*
            string lastFile = "";
            Directory.CreateDirectory("PCAPOutput");
            Directory.CreateDirectory("JSONOutput");
            //Console.Out.WriteLine("Hello");
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //process.StartInfo.UseShellExecute = false;
            //process.StartInfo.RedirectStandardOutput = true;
            //IMPORTANT: make sure that the tshark exe is is the following path
            //TODO: put a check to make sure it is in the path
            //startInfo.FileName = @"C:\Program Files\Wireshark\tshark.exe";
            //startInfo.Arguments = @"-i 2 -b duration:6 -b files:5 -T json > json";
            process.StartInfo.FileName = @"cmd.exe";
            process.StartInfo.Arguments = @"/K ""C:\Program Files\Wireshark\tshark.exe"" -i 2 -c 100 -T json > output.json";

            //process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();
            //Console.Out.WriteLine(process.StandardOutput.ReadToEnd());

            /*var directory = new DirectoryInfo("PCAPOutput");
            string tempFileName = "";
            while (true)
            {
                //get most recently written file
                //got this code block from https://stackoverflow.com/questions/1179970/how-to-find-the-most-recent-file-in-a-directory-using-net-and-without-looping
                try
                {
                    var myFile = (from file in directory.GetFiles() orderby file.LastWriteTime descending select file).ElementAt(2);
                    tempFileName = myFile.Name;
                }
                catch (ArgumentOutOfRangeException)
                {
                    Thread.Sleep(6000);
                    continue;
                }
                //end of credited code block

                //create thread to process tempFile if filename is not the last one seen (linear); otherwise, sleep for a bit
                if (lastFile.Equals(tempFileName))
                {
                    Thread.Sleep(500);
                    continue;
                }
                else
                {
                    File.Copy(@"PCAPOutput\" + tempFileName, "curJSON.pcap", true);
                    try
                    {   // Open the text file using a stream reader.
                        //using (StreamReader sr = new StreamReader(@"PCAPOutput\" + tempFileName))
                        //{
                            //Console.Out.WriteLine("Opened" + tempFileName);
                            // Read the stream to a string, and write the string to the console.
                            //String line = sr.ReadToEnd();
                            //Console.WriteLine(line);
                            //sr.Close();
                            //Console.Out.WriteLine("Closed" + tempFileName);
                        //}
                        //Write to JSON file from pcap file
                        Process process1 = new Process();
                        //ProcessStartInfo startInfo1 = new ProcessStartInfo();
                        process1.StartInfo.CreateNoWindow = false;
                        process1.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                        process1.StartInfo.RedirectStandardOutput = true;
                        process1.StartInfo.UseShellExecute = false;
                        //IMPORTANT: make sure that the tshark exe is is the following path
                        //TODO: put a check to make sure it is in the path
                        process1.StartInfo.FileName = @"C:\Program Files\Wireshark\tshark.exe";
                        process1.StartInfo.Arguments = "-r \"curJSON2.pcap\" -T json > \"output.json\"";
                        process1.Start();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("The file could not be read:");
                        Console.WriteLine(e.Message);
                    }

                }

                lastFile = tempFileName;
            }
            //"C:\Program Files\Wireshark\tshark.exe" -i 2 -a duration:5 -w output.pcap
            //"C:\Program Files\Wireshark\tshark.exe" -r output.pcap -T json > output.json
            //C:\Users\Patrick\Source\Repos\CSE4471Project\WiresharkApp\WiresharkApp\Output\output.pcap
            //Directory.Delete("Output", true);

            //create a buffer ring of files
                //switch files every 5 seconds
            //run constant process that looks for most recent file; if not yet seen, copy it to new file and pass into new thread that processes json into database
                //if file already seen, then wait for half second before checking again*/
        }
    }
}
