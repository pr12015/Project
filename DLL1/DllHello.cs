using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace DLL1
{
    public class DllHello : Contracts.IWorkerRole
    {
        Contracts.IRoleEnvironment proxy;
        string address;

        public DllHello() { }

        public void Start(string containerId)
        {
            NetTcpBinding binding = new NetTcpBinding();
            var factory = new ChannelFactory<Contracts.IRoleEnvironment>(binding, new EndpointAddress($"net.tcp://localhost:{10000}/RoleEnvironment"));
            proxy = factory.CreateChannel();

            address = proxy.GetAddress("DLL1.dll", containerId);

            Console.WriteLine(address);

            Greet();
        }

        public void Stop()
        {

        }

        public void Greet() { Console.WriteLine("Hello from dll hell"); }
    }
}
