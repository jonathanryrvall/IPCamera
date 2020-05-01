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


   

      

     


        /// <summary>
        /// Config file
        /// </summary>
        public static string ConfigPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory + "/config.xml";
        }

    }
}
