using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Contracts;
using System.ServiceModel;

namespace ComputeService
{
    class Service
    {
        private IContainer proxy;
        private int instances;

        // To determine when to shutdown the program
        private static volatile bool exit = false;

        public Service(int instances) { this.instances = instances; }

        public void Start()
        {
            // lacations and filename of the dll package to be copied
            string fileName = "", src = "";
            string dest = @"C:\users\stefan\Desktop\containers\Container";

            Task.Factory.StartNew(() =>
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
                exit = true;
            });

            while (!exit)
            {
                if (XmlHelper.Changed)
                {
                    // Only 4 instances are allowed.
                    if(XmlHelper.Instances > 4)
                    {
                        XmlHelper.Changed = false;
                        throw new Exception("Cannot have more than 4 instances. Change the congfig file.");
                    }

                    var files = Directory.GetFiles(@XmlHelper.PackageLocation, "*.dll");

                    // Extract the dll name.
                    if (files.Length == 1)
                    {
                        fileName = files[0].Split('\\')[5];
                        src = files[0];
                    }
                    else if (files.Length > 1)
                    {
                        Console.WriteLine("ERROR: Only one dll at the time.");
                    }
                    else
                    {
                        Console.WriteLine("ERROR: Dll file missing from package.");
                    }

                    // Copy the dll, and call Load().
                    for (int i = 0; i < XmlHelper.Instances; ++i)
                    {
                        string assemblyDest = dest + (i + 1) + "\\" + fileName;
                        try
                        {
                            File.Copy(src, assemblyDest);
                        }
                        catch (Exception e)
                        {
                            // dll already copied.
                            // Catching exception to prevent program from breaking.
                            // Console.WriteLine(e.Message);
                        }

                        try
                        {
                            Connect(i);
                            string containerResponse = proxy.Load(assemblyDest);
                            Console.WriteLine(containerResponse);
                            System.Threading.Thread.Sleep(200);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    }
                    XmlHelper.Changed = false;
                }
            }
        }
        
        // Connect to Containers WCF server
        public void Connect(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            var factory = new ChannelFactory<IContainer>(binding, new EndpointAddress(string.Format("net.tcp://localhost:{0}/Container", 10010 + (id * 10))));

            proxy = factory.CreateChannel();
        }

        public void Stop()
        {
            foreach (Process p in Program.processes)
            {
                try
                {
                    p.Kill();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            for (int i = 1; i < 5; ++i)
                DeleteFile(i);
            
            foreach(string dir in Directory.GetDirectories(@"C:\users\stefan\Desktop\containers\"))            
                Directory.Delete(dir);
        }

        public void DeleteFile(int id)
        {
            File.Delete(@"C:\users\stefan\Desktop\containers\Container" + id + @"\DLL1.dll");
        }

        //public void Stop(int id)
        //{
        //    //File.SetAttributes(@"C:\users\stefan\Desktop\containers\Container" + id, FileAttributes.Normal);
        //    File.Delete(@"C:\users\stefan\Desktop\containers\Container" + id + "DLL1.dll");
        //    Directory.Delete(@"C:\users\stefan\Desktop\containers\Container" + id);
        //}
    }
}
