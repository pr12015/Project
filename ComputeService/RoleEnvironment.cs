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
        bool firstPass = true;

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

        /// <summary>
        ///     Returns list of 'brother-containers'.
        ///     Containers are identified by containerId
        ///     which is the port opened by container.
        /// </summary>
        /// <param name="myAssemblyName"> Dll name </param>
        /// <param name="myAddress"> dll location </param>
        /// <returns> List of 'brother-containers' </returns>
        public string[] BrotherInstances(string myAssemblyName, string myAddress)
        {
            var dirs = Directory.GetDirectories(containerPath);
            string[] brotherInstances = new string[3];
            int j = 0, idx = 5;

            foreach(string d in dirs)
            {
                var files = Directory.GetFiles(d);

                string container = d.Split('\\')[5];
                int i = Int32.Parse(container.Last().ToString());

                --i;

                if (files.Contains(myAddress + "\\" + myAssemblyName))
                {
                    idx = i;
                    continue;
                }
                else if (firstPass)
                {
                    
                    if (Service.Busy[i] && i != idx && j < XmlHelper.Instances - 1)
                    {
                        string containerId = (10011 + i * 10).ToString();
                        brotherInstances[j++] = containerId;
                    }
                }
                else
                {
                    for(int k = 0; k < XmlHelper.Instances; ++k)
                    {
                        if(k != idx && Service.Busy[k])
                        {
                            string containerId = (10011 + i * 10).ToString();
                            brotherInstances[j++] = containerId;
                        }
                    }
                }
            }

            if (firstPass)
                firstPass = false;

            return brotherInstances;
        }
    }
}
