using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL1
{
    public class DllHello : Contracts.IWorkerRole
    {
        public DllHello() { }

        public void Start()
        {
            Greet();
        }

        public void Stop()
        {

        }

        public void Greet() { Console.WriteLine("Hello from dll hell"); }
    }
}
