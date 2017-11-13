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
            Directory.CreateDirectory("Output");
            //Console.Out.WriteLine("Hello");
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //IMPORTANT: make sure that the tshark exe is is the following path
            //TODO: put a check to make sure it is in the path
            startInfo.FileName = @"C:\Program Files\Wireshark\tshark.exe";
            startInfo.Arguments = @"-i 2 -a duration:6 -w Output\output.pcap";
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();

            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"Output\output.pcap"))
                {
                    // Read the stream to a string, and write the string to the console.
                    String line = sr.ReadToEnd();
                    Console.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
            Thread.Sleep(5000);
            //"C:\Program Files\Wireshark\tshark.exe" -i 2 -a duration:5 -w output.pcap
            //"C:\Program Files\Wireshark\tshark.exe" -r output.pcap -T json > output.json
            //C:\Users\Patrick\Source\Repos\CSE4471Project\WiresharkApp\WiresharkApp\Output\output.pcap

            //create a buffer ring of files
                //switch files every 5 seconds
            //run constant process that looks for most recent file; if not yet seen, copy it to new file and pass into new thread that processes json into database
                //if file already seen, then wait for half second before checking again
        }
    }
}
