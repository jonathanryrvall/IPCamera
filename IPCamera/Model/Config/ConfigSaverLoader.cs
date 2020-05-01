using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace IPCamera.Model.Config
{
    public static class ConfigSaverLoader
    {
        private static List<string> delayedSaveLocks;

            

        /// <summary>
        /// Load configuration
        /// </summary>
        public static T Load<T>(string path) where T : IConfig, new()
        {
            // File exists?
            if (!File.Exists(path))
            {
                T config = new T();
                (config as IConfig).AddMissingCascade();
                return config;
            }

            // Read file
            string xml = File.ReadAllText(path);

            // Deserialize
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(xml))
            {
                T config = (T)xsSubmit.Deserialize(reader);
                (config as IConfig).AddMissingCascade();
                return config;
            }
        }

        /// <summary>
        /// Save configuration after a delay, the save configuration cannot be saved during this delay
        /// </summary>
        public static void DelayedSave(object config, string path, int ms = 800)
        {
            // Create list of locked paths that cannot be saved during delay
            if (delayedSaveLocks == null)
            {
                delayedSaveLocks = new List<string>();
            }

            // Check if lock exists
            if (delayedSaveLocks.Contains(path))
            {
                return;
            }

            // Add lock
            delayedSaveLocks.Add(path);

            // Create new timer
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
            timer.Tick += (sender, args) =>
            {
                // Save
                Save(config, path);

                // Remove lock
                delayedSaveLocks.Remove(path);

                // Stop timer to allow gc
                timer.Stop();
            };


            timer.Start();
        }

        /// <summary>
        /// Save configureation
        /// </summary>
        public static void Save(object config, string path)
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.NewLineChars = "\r\n";
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "   ";
            xmlSettings.NewLineHandling = NewLineHandling.Replace;
            XmlSerializer xsSubmit = new XmlSerializer(config.GetType());
            using (var sww = new StreamWriter(path))
            {
                using (XmlWriter writer = XmlWriter.Create(sww, xmlSettings))
                {
                    xsSubmit.Serialize(writer, config);
                }
            }
        }

    }
}
