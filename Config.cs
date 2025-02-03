using System;
using System.IO;
using System.Xml.Linq;

namespace GPSTracker
{
    public static class Config
    {
        public static string PathCacheByond { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Personal) + @"\BYOND\cache\tmp";

        public static string PathGpsData { get; set; } = "GPSData.txt";

        public static string PathOpenWormhole { get; set; } = "../open_wormhole.txt";

        public static void SaveConfig()
        {
            XElement element = new XElement("Config");
            XElement el3 = new XElement("PathCacheByond");
            el3.Value = PathCacheByond;
            element.Add(el3);
        
            XElement el2 = new XElement("PathGPSData");
            el2.Value = PathGpsData;
            element.Add(el2);
            
            XElement el1 = new XElement("PathOpenWormhole");
            el1.Value = PathOpenWormhole;
            element.Add(el1);

            element.Save("config.xml");
        }

        public static void LoadConfig()
        {
            StreamReader reader = new StreamReader("config.xml");
            XElement element = XElement.Load(reader);
            reader.Close();

            PathCacheByond = element.Element("PathCacheByond")?.Value ?? $"{PathCacheByond}";
        
            PathGpsData = element.Element("PathGPSData")?.Value ?? $"{PathGpsData}";
            
            PathOpenWormhole = element.Element("PathOpenWormhole")?.Value ?? $"{PathOpenWormhole}";

            element.Save("config.xml");
        }
    }
}