using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model
{
    public static class FilePaths
    {

        static string startupPath = AppDomain.CurrentDomain.BaseDirectory;


        public static string RecordPath()
        {
            if (!Directory.Exists(startupPath + "record"))
                Directory.CreateDirectory(startupPath + "record");

            return startupPath + "record";
        }

        public static string TimelapsePath()
        {
            if (!Directory.Exists(startupPath + "timelapse"))
                Directory.CreateDirectory(startupPath + "timelapse");

            return startupPath + "timelapse";
        }


        public static string LogsPath()
        {
            if (!Directory.Exists(startupPath + "logs"))
                Directory.CreateDirectory(startupPath + "logs");

            return startupPath + "logs";
        }






        /// <summary>
        /// Config file
        /// </summary>
        public static string ConfigPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "/config.xml";
        }

    }
}
