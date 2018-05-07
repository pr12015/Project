using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Contracts
{
    [ServiceContract]
    public interface IRoleEnvironment
    {
        [OperationContract]
        string GetAddress(string myAssemblyName, string containerId);

        [OperationContract]
        string[] BrotherInstances(string myAssemblyName, string myAddress);
    }
}
