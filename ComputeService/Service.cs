using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using Contracts;
using System.ServiceModel;
using System.Threading;

namespace ComputeService
{
    public class Service
    {
        private IContainer[] proxy = new IContainer[4];
        private int instances;
        private static bool[] busy = new bool[4];
        private bool stopChecking = false;

        // To determine when to shutdown the program
        private static volatile bool exit = false;

        public static bool[] Busy { get { return busy; } }

        public Service(int instances) { this.instances = instances; }
        
        public void Start()
        {
            ConnectAll();

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
                    if (XmlHelper.Instances > 4)
                    {
                        XmlHelper.Changed = false;
                        throw new Exception("Cannot have more than 4 instances. Change the congfig file.");
                    }

                    DeleteOnRelaunch();

                    bool loadResult = LoadDll();

                    if (!loadResult)
                        continue;

                    for (int i = 0; i < 4; ++i)
                        Console.WriteLine($"Container {i} is: " + (busy[i] ? "busy" : "not busy"));

                    XmlHelper.Changed = false;
                }
            }
        }

        #region METHODS
        /// <summary>
        ///     Copies the dll to container location.
        ///     Loads the dll.
        /// </summary>
        /// <returns> true if everything was successfull </returns>
        public bool LoadDll()
        {
            string dest = @"C:\users\stefan\Desktop\containers\Container";

            var tuple = ExtractDllInfo();

            if (tuple == null)
                return false;

            string src = tuple.Item1;
            string fileName = tuple.Item2;

            // Copy the dll, and call Load().
            for (int i = 0; i < 4; ++i)
            {
                int j = i;
                Task.Factory.StartNew(() => Alive(j));
                if (i < XmlHelper.Instances)
                {
                    busy[i] = true;
                    string assemblyDest = CopyDll(src, fileName, dest, i);
                    try
                    {
                        //Connect(i);
                        //Task.Factory.StartNew(() => Alive(i));
                        string containerResponse = proxy[i].Load(assemblyDest);
                        Console.WriteLine(containerResponse);
                        System.Threading.Thread.Sleep(200);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        ///     LoadDll() overload. 
        ///     Performs copy and load for i-th container.
        /// </summary>
        public bool LoadDll(int i)
        {
            string dest = @"C:\users\stefan\Desktop\containers\Container";

            var tuple = ExtractDllInfo();

            if (tuple == null)
                return false;

            string src = tuple.Item1;
            string fileName = tuple.Item2;

            busy[i] = true;

            string assemblyDest = CopyDll(src, fileName, dest, i);
            try
            {
                // Alive is not called, since it's already running.
                // Task.Factory.StartNew(() => Alive(i));
                string containerResponse = proxy[i].Load(assemblyDest);
                Console.WriteLine(containerResponse);
                System.Threading.Thread.Sleep(200);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            for (int j = 0; j < 4; ++j)
                Console.WriteLine($"Container {j} is: " + (busy[j] ? "busy" : "not busy"));

            return true;
        }

        /// <summary>
        ///     Connects to the i-th container.
        /// </summary>
        /// <param name="id"> i-th container </param>
        public void Connect(int id)
        {
            NetTcpBinding binding = new NetTcpBinding();
            var factory = new ChannelFactory<IContainer>(binding, new EndpointAddress(string.Format("net.tcp://localhost:{0}/Container", 10011 + (id * 10))));

            proxy[id] = factory.CreateChannel();
        }

        /// <summary>
        ///     Connects to all containers.
        /// </summary>
        public void ConnectAll()
        {
            NetTcpBinding binding = new NetTcpBinding();
            for (int i = 0; i < 4; ++i)
            {
                var factory = new ChannelFactory<IContainer>(binding, new EndpointAddress(string.Format("net.tcp://localhost:{0}/Container", 10011 + (i * 10))));
                proxy[i] = factory.CreateChannel();
            }
        }

        /// <summary>
        ///     Extracts the source and the name of the dll
        /// </summary>
        /// <returns>
        ///     Item1 = source
        ///     Item2 = dllName
        /// </returns>
        public Tuple<string, string> ExtractDllInfo()
        {
            var files = Directory.GetFiles(@XmlHelper.PackageLocation, "*.dll");
            string src, dllName;

            // Extract the dll name.
            if (files.Length == 1)
            {
                dllName = files[0].Split('\\')[5];
                src = files[0];
            }
            else if (files.Length > 1)
            {
                Console.WriteLine("ERROR: Only one dll at the time.");
                return null;
            }
            else
            {
                Console.WriteLine("ERROR: Dll file missing from package.");
                return null;
            }
            return new Tuple<string, string>(src, dllName);
        }

        /// <summary>
        ///     Copies the dll to i-th container.    
        ///     returns location of the dll.
        /// </summary>
        /// <param name="src"> Source location of the dll </param>
        /// <param name="dllName"> dll name </param>
        /// <param name="dst"> Incomplete destination container location of the copy of the dll </param>
        /// <param name="i"> Determines the specific container location </param>
        /// <returns>
        ///     Full path to the dll.
        /// </returns>
        public string CopyDll(string src, string dllName, string dst, int i)
        {
            string assemblyDest = dst + (i + 1) + "\\" + dllName;
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
            return assemblyDest;
        }

        /// <summary>
        ///    Check if i-th container is alive. Faulty connection 
        ///    indicates faulty container, thus i-th container is 
        ///    marked not alive.
        ///    If container dies, resurrection is performed according
        ///    to the rules.
        /// </summary>
        /// <param name="i"> i-th container </param>
        public void Alive(int i)
        {
            bool resurrected = false;

            while (!stopChecking)
            {
                try
                {
                    if (resurrected)
                    {
                        Connect(i);
                        resurrected = false;
                    }
                    proxy[i].CheckState();
                }
                catch (Exception e)
                {
                    // TODO: Fault-tolerance logic

                    // If the faulty container is not running a dll.
                    if (!busy[i])
                    {
                        try
                        {
                            if (!resurrected)
                            {
                                Program.processes[i].Start();
                                resurrected = true;
                            }
                        }
                        catch (Exception e1)
                        {
                            Console.WriteLine(e1.Message);
                        }
                    }
                    else
                    {
                        busy[i] = false;
                        // Check for container that is not running a dll.
                        // If found run the dll, then restart the container.
                        for (int j = 0; j < 4; ++j)
                        {
                            if (!busy[j] && j != i)
                            {
                                LoadDll(j);
                                try
                                {
                                    if (!resurrected)
                                    {
                                        Program.processes[i].Start();
                                        resurrected = true;

                                        break;
                                    }
                                }
                                catch (Exception e1)
                                {
                                    Console.WriteLine(e1.Message);
                                }
                            }
                        }

                        // If there was no free container
                        if (!resurrected)
                        {
                            try
                            {
                                Program.processes[i].Start();
                                Connect(i);
                                resurrected = true;
                            }
                            catch (Exception e1)
                            {
                                Console.WriteLine(e1.Message);
                            }
                            LoadDll(i);
                        }
                    }
                }
                //Connect(i);
                Task.Delay(1500);
            }
        } 

        /// <summary>
        ///     Stops the checking task.
        ///     Stops containers.
        ///     Deletes dlls and container directories.
        /// </summary>
        public void Stop()
        {
            object o = new object();
            lock (o)
            {
                stopChecking = true;
            }

            foreach (Process p in Program.processes)
            {
                try
                {
                    p.CloseMainWindow();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            // Delete dlls.
            for (int i = 1; i < 5; ++i)
                DeleteFile(i);
            
            // Delete containers.
            foreach(string dir in Directory.GetDirectories(@"C:\users\stefan\Desktop\containers\"))            
                Directory.Delete(dir);
        }

        public void DeleteFile(int id)
        {
            File.Delete(@"C:\users\stefan\Desktop\containers\Container" + id + @"\DLL1.dll");
        }

        /// <summary>
        ///     Deletes dlls if number of instances is changed
        /// </summary>
        public void DeleteOnRelaunch()
        {
            try
            {
                for (int i = 0; i < 4; ++i)
                    DeleteFile(i);
            }
            catch (Exception e)
            {
                // If there is no file to delete.
                // No need to print this information.
            }
        }
        #endregion
    }
}
