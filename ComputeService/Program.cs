﻿using System;
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

        // Paths used in ComputeService
        public static string containerAppPath = @"C:\Users\stefan\Downloads\HelloWorld-master\HelloWorld\bin\Debug\HelloWorld.exe";
        public static string containerPath = @"C:\users\stefan\Desktop\containers";
        public static string specificContainerPath = containerPath + @"\Container";


        static void Main(string[] args)
        {           
            Console.WriteLine("Press any key to start service. . .");
            Console.ReadKey(true);

            Start();

            Console.WriteLine("Service started successfully!");

            XmlHelper xmlHelper = new XmlHelper();

            Task readAsync = xmlHelper.AsyncRead();
            //readAsync.Wait();
            Service service = new Service(XmlHelper.Instances);
            bool serviceSuccess = false;
            while (!serviceSuccess)
            {
                try
                {
                    service.Start();
                    serviceSuccess = true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            service.Stop();

            Console.WriteLine("press any key to exit...");
            Console.ReadKey(true);
        }

        // start 4 container apps (console apps) and create a direcotry for each
        public static void Start()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = containerAppPath,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            Directory.CreateDirectory(containerPath);

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
                    Directory.CreateDirectory(specificContainerPath + (i + 1));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


    }
}
