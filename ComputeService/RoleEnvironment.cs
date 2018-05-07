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
            int j = 0;

            foreach(string d in dirs)
            {
                var files = Directory.GetFiles(d);
                if (files.Contains(myAddress + "\\" + myAssemblyName))
                    continue;

                string container = d.Split('\\')[5];
                int i = Int32.Parse(container.Last().ToString());

                --i;

                string containerId = (10011 + i * 10).ToString();
                if(j < XmlHelper.Instances)
                    brotherInstances[j++] = containerId;
            }

            return brotherInstances;
        }
    }
}
