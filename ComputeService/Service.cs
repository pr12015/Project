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

        public Service() { }

        public void Start()
        {
            string fileName = "", src = "", dest = "";
            while (true)
            {
                if (XmlHelper.changed)
                {                   
                    var files = Directory.GetFiles(@XmlHelper.packageLocation, "*.dll");
                    if (files.Length == 1)
                    {
                        fileName = files[0].Split('\\')[5];
                        src = files[0];
                        dest = @"C:\users\stefan\Desktop\containers\Container";
                    }
                    else if (files.Length > 1)
                        Console.WriteLine("ERROR: Onlu one dll at the time.");
                    else
                        Console.WriteLine("ERROR: Dll file missing from package.");
                    for (int i = 0; i < XmlHelper.instances; ++i)
                    {
                        string __dest = dest + (i + 1) + "\\" + fileName;
                        File.Copy(src, __dest);
                        Connect(i);
                        proxy.Load((i + 1) + "\\" + fileName); // (i + 1) to load from appropriate folder
                    }
                    DeleteFiles(XmlHelper.packageLocation);
                    XmlHelper.changed = false;
                }
            }
        }

        /*
        public void Start()
        {
            Process[] processes = new Process[4];
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "C:\\Users\\stefan\\Desktop\\Container.exe",
                WindowStyle = ProcessWindowStyle.Minimized
            };

            Directory.CreateDirectory(@"C:\users\stefan\Desktop\containers");

            int port = 10010;

            for (int i = 0; i < 4; ++i)
            {
                try
                {
                    //process.StartInfo.UseShellExecute = true;
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
        }*/
        
        public void Stop()
        {
            for (int i = 1; i <= 4; ++i)
                Directory.Delete(@"C:\users\stefan\Desktop\containers\Container" + i);
        }

        public void DeleteFiles(string uri)
        {
            string[] files = Directory.GetFiles(uri);
            foreach(string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
        }

        // Conect to Containers WCF server
        public void Connect(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            var factory = new ChannelFactory<IContainer>(binding, new EndpointAddress(string.Format("net.tcp://localhost:{0}/Container", 10010 + (id * 10))));

            proxy = factory.CreateChannel();
        }
    }
}
