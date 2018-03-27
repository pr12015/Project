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
    class Service : IWorkerRole
    {
        private IContainer proxy;
        static volatile bool exit = false;

        public Service() { }

        public void Start()
        {
            string fileName = "", src = "", dest = "";

            Task.Factory.StartNew(() =>
            {
                while (Console.ReadKey(true).Key != ConsoleKey.Q) ;
                exit = true;
            });

            while (!exit)
            {
                if (XmlHelper.changed || fileName != "")
                {
                    var files = Directory.GetFiles(@XmlHelper.packageLocation, "*.dll");
                    if (files.Length == 1)
                    {
                        fileName = files[0].Split('\\')[5];
                        src = files[0];
                        dest = @"C:\users\stefan\Desktop\containers\Container";
                    }
                    else if (files.Length > 1)
                        Console.WriteLine("ERROR: Only one dll at the time.");
                    else
                        Console.WriteLine("ERROR: Dll file missing from package.");

                    for (int i = 0; i < XmlHelper.instances; ++i)
                    {
                        string __dest = dest + (i + 1) + "\\" + fileName;
                        try
                        {
                            File.Copy(src, __dest);
                        }
                        catch (Exception e) { }

                        try
                        {
                            Connect(i);
                            proxy.Load((i + 1) + "\\" + fileName); // (i + 1) to load from appropriate folder
                            File.Delete(dest + (i + 1) + "\\" + fileName);
                            System.Threading.Thread.Sleep(200);
                        }
                        catch(Exception e) { Stop(i + 1); }
                    }
                    //DeleteFiles(XmlHelper.packageLocation);
                    XmlHelper.changed = false;
                }
            }
        }
        
        public void Stop()
        {
            foreach (Process p in Program.processes)
                p.CloseMainWindow();
                //p.Close();
            

            foreach(string dir in Directory.GetDirectories(@"C:\users\stefan\Desktop\containers\"))
            {
                Directory.Delete(dir);
            }
        }
        public void Stop(int id)
        {
            File.Delete(@"C:\users\stefan\Desktop\containers\Container" + id);
            Directory.Delete(@"C:\users\stefan\Desktop\containers\Container" + id);
        }

        // Connect to Containers WCF server
        public void Connect(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            var factory = new ChannelFactory<IContainer>(binding, new EndpointAddress(string.Format("net.tcp://localhost:{0}/Container", 10010 + (id * 10))));

            proxy = factory.CreateChannel();
        }
    }
}
