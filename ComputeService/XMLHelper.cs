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
        public static string packageLocation;
        public static int instances = 0;
        public static bool changed = false;

        public XmlHelper()
        {
            using (XmlReader reader = XmlReader.Create(@"C:\Users\stefan\Desktop\Project\ComputeService\package.xml"))
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "location")
                    {
                        packageLocation = reader.ReadElementContentAsString();
                        break;
                    }
                }
            }

            dirInfo = new DirectoryInfo(@packageLocation);

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

        // string PackageLocation { get { return packageLocation; } }

        private async Task Read(string inputUri)
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = true;

            using (XmlReader reader = XmlReader.Create(inputUri, settings))
            {
                while (await reader.ReadAsync())
                {
                    if (reader.NodeType == XmlNodeType.Element && reader.Name == "instances")
                        instances = reader.ReadElementContentAsInt();
                }
            }
        }

        public async Task<int> AsyncRead()
        {
            while (true)
            {

                FileInfo[] files = dirInfo.GetFiles("*.xml");
                if (files.Length == 1)
                {
                    if (files[0].Name != fileName)
                    {
                        fileName = files[0].Name;
                        changed = true;
                    }

                    Task readTask = Read(files[0].FullName);

                    await readTask;
                }
                else if (files.Length > 1)
                {
                    Console.WriteLine("ERROR: Only 1 config file allowed.");
                    Console.WriteLine("Choose one.");
                }
                else
                {
                    Console.WriteLine("WARNING: Config file could not be found.");
                    Console.WriteLine("Add a config file to {0}", dirInfo.FullName); // dirInfo.FullName should be replaced with configurable xml element value!
                }
                await Task.Delay(1000);
            }
        }
    }
}
