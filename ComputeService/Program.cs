using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using Contracts;
using System.Diagnostics;
using System.IO;

namespace ComputeService
{
    class Program
    {
        public static Process[] processes = new Process[4];


        static void Main(string[] args)
        {           
            Service service = new Service();
            Console.WriteLine("Press any key to start service. . .");
            Console.ReadKey(true);

            Start();



            Console.WriteLine("Service started successfully!");

            XmlHelper xmlHelper = new XmlHelper();

            Task readAsync = xmlHelper.AsyncRead();

            service.Start();
            service.Stop();

            Console.WriteLine("press any key to exit...");
            Console.ReadKey(true);
        }

        // start 4 container apps (console apps) and create a direcotry for each
        public static void Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = @"C:\Users\stefan\Desktop\HelloWorld\HelloWorld\bin\Debug\HelloWorld.exe",
                WindowStyle = ProcessWindowStyle.Minimized
            };

            Directory.CreateDirectory(@"C:\users\stefan\Desktop\containers");

            int port = 10010;

            for (int i = 0; i < 4; ++i)
            {
                try
                {
                    startInfo.UseShellExecute = true;
                    startInfo.Arguments = (port + i * 10).ToString();
                    processes[i] = new Process
                    {
                        StartInfo = startInfo
                    };
                    processes[i].Start();
                    Directory.CreateDirectory(@"C:\users\stefan\Desktop\containers\Container" + (i + 1));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


    }
}
