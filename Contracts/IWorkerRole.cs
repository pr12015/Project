using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IWorkerRole
    {
        void Start(string containerId);
        void Stop();
    }
} // namespace Contracts
