using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Reporter
{
    static class Settings
    {
        public static string OpcDaIpAddress { get; set; }
        public static string OpcDaServerName { get; set; }

        public static string OpcHdaIpAddress { get; set; }
        public static string OpcHdaServerName { get; set; }

        public static bool IncludeBounds { get; set; }

        public static DateTime StartTime { get; set; }
        public static DateTime EndTime { get; set; }

        static Settings()
        {
            try
            {
                var iniFile = File.ReadAllLines("Reporter.ini").ToList();
                var daIndex = iniFile.IndexOf("[Da]");
                var daConnSettings = iniFile.GetRange(daIndex + 1, 2);
                foreach (var line in daConnSettings)
                {
                    var lineParts = line.Split('=');
                    if (lineParts[0].Trim() == "Ip")
                        OpcDaIpAddress = lineParts[1].Trim();
                    if (lineParts[0].Trim() == "Server")
                        OpcDaServerName = lineParts[1].Trim();
                }

                var hdaIndex = iniFile.IndexOf("[Hda]");
                var hdaConnSettings = iniFile.GetRange(hdaIndex + 1, 2);
                foreach (var line in hdaConnSettings)
                {
                    var lineParts = line.Split('=');
                    if (lineParts[0].Trim() == "Ip")
                        OpcHdaIpAddress = lineParts[1].Trim();
                    if (lineParts[0].Trim() == "Server")
                        OpcHdaServerName = lineParts[1].Trim();
                }
            }
            catch 
            {
                OpcDaIpAddress = "localhost";
                OpcDaServerName = "Elesy.DualSource";

                OpcHdaIpAddress = "localhost";
                OpcHdaServerName = "Webrouter.OPCHDA";
            }
        }
    }
}
