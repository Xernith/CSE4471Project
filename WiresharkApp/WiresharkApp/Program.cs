using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            //Console.Out.WriteLine("Hello");
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            //IMPORTANT: make sure that the tshark exe is is the following path
            //TODO: put a check to make sure it is in the path
            startInfo.FileName = @"C:\Program Files\Wireshark\tshark.exe";
            startInfo.Arguments = "-i 2";
            process.StartInfo = startInfo;
            process.Start();
        }
    }
}
