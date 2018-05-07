using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using System.ServiceModel;

namespace ComputeService
{
    class Server
    {
        private ServiceHost serviceHost;

        public Server()
        {
            Start();
        }

        public int Port { get; set; } = 10000;

        public void Start()
        {
            serviceHost = new ServiceHost(typeof(RoleEnvironment));
            NetTcpBinding binding = new NetTcpBinding();
            serviceHost.AddServiceEndpoint(typeof(IRoleEnvironment), binding, new Uri(string.Format("net.tcp://localhost:{0}/RoleEnvironment", Port)));

            serviceHost.Open();
            Console.WriteLine("Server (PORT: {0}) ready and waiting for requests...", Port);
        }

        public void Stop()
        {
            serviceHost.Close();
            Console.WriteLine("Server has been closed.");
        }
    }
}
