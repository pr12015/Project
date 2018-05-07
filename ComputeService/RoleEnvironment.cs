using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using System.Diagnostics;
using System.IO;
//using System.IO;

namespace ComputeService
{
    public class RoleEnvironment : IRoleEnvironment
    {
        public static string containerPath = @"C:\users\stefan\Desktop\containers";

        /// <summary>
        ///     Finds the address of the dll running in container
        ///     by ectracting the location sufix of i-th container.
        ///     Location is returned only if it contains the dll.
        /// </summary>
        /// <param name="myAssemblyName"> Dll name. </param>
        /// <param name="containerId"> Container's port. </param>
        /// <returns> Location of the dll </returns>
        public string GetAddress(string myAssemblyName, string containerId)
        {
            int sufix = (Int32.Parse(containerId) - 10011)/ 10 + 1;
            var dirs = Directory.GetDirectories(containerPath);
            foreach (Process p in Program.processes)
            {
                if (p.StartInfo.Arguments.Equals(containerId))
                {
                    foreach(string dirPath in dirs)
                    {
                        var files = Directory.GetFiles(dirPath);
                        if (dirPath.Contains(sufix.ToString()) && files.Contains(dirPath + "\\" + myAssemblyName))
                            return dirPath;
                    }

                }
            }
            return null;
        }

        public string[] BrotherInstances(string myAssemblyName, string myAddress)
        {
            return null;
        }
    }
}
