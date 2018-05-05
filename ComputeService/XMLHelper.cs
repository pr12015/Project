using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace ComputeService
{
    public class XmlHelper
    {
        private string fileName;
        private DirectoryInfo dirInfo;
        private static int instances;

        #region Properties
        public static string PackageLocation { get; set; }
        public static int Instances { get { return instances; } set { instances = value; } }
        public static bool Changed { get; set; } = false;
        #endregion

        // Constructor
        // Finds the location of the config file by reading `package.xml`
        public XmlHelper()
        {
            instances = 0;
            using (XmlReader reader = XmlReader.Create(@"C:\Users\stefan\Desktop\Project\ComputeService\package.xml"))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "location")
                    {
                        PackageLocation = reader.ReadElementContentAsString();
                        break;
                    }                    
                }
            }

            dirInfo = new DirectoryInfo(PackageLocation);

            try
            {
                if (!dirInfo.Exists)
                    dirInfo.Create();
            }
            catch (Exception e)
            {
                Console.WriteLine("Could not create directory. Reason: {0}", e.ToString());
            }
        }


        // Reads the config file and stores the number of instances.
        private void Read(string inputUri)
        {
            using (XmlReader reader = XmlReader.Create(inputUri))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "instances")
                    {
                        int instanceNumber = reader.ReadElementContentAsInt();
                        if (instances != instanceNumber)
                        {
                            instances = instanceNumber;
                            Changed = true;
                            //if (reader.ReadElementContentAsInt() < 5)
                            //{
                            //    instances = reader.ReadElementContentAsInt();
                            //    changed = true;
                            //}
                            //else
                            //{
                            //    // throw new Exception("Cannot have more than 4 instances. Change the congfig file.");
                            //    Console.WriteLine("Cannot have more than 4 instances. Change the congfig file.");
                            //}
                        }
                                                    
                        break;
                    }
                }
            }
        }

        
        // Checks the predefined location for changes in config file.       
        public async Task AsyncRead()
        {
            while (true)
            {
                // changed = false;
                FileInfo[] files = dirInfo.GetFiles("*.xml");
                if (files.Length == 1)
                {
                    if (files[0].Name != fileName)
                    {
                        fileName = files[0].Name;
                        Changed = true;
                    }

                    Read(files[0].FullName); 
                }
                else if (files.Length > 1)
                {
                    Console.WriteLine("ERROR: Only 1 config file allowed.");
                    Console.WriteLine("Choose one.");
                }
                else
                {
                    Console.WriteLine("WARNING: Config file could not be found.");
                    Console.WriteLine("Add a config file to {0}", dirInfo.FullName);
                }
                await Task.Delay(1000);
            }
        }
    }
}
