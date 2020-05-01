using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace IPCamera.Model.Config
{
    /// <summary>
    /// Config object interface
    /// </summary>
    public interface IConfig
    {
        void AddMissing();
    }

    public static class ConfigExtensions
    {


        /// <summary>
        /// Clamp a comparable value
        /// </summary>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable
        {
            if (value.CompareTo(max) > 0)
            {
                return max;
            }
            if (value.CompareTo(min) < 0)
            {
                return min;
            }
            return value;
        }

        /// <summary>
        /// Clone
        /// </summary>
        public static T Clone<T>(this T config) where T : IConfig
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            XmlSerializer xsSubmit = new XmlSerializer(config.GetType());

            string xml = "";
            using (var sw = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, xmlSettings))
                {
                    xsSubmit.Serialize(writer, config);
                }
                xml = sw.ToString();
            }

            using (StringReader reader = new StringReader(xml))
            {
                return (T)xsSubmit.Deserialize(reader);
            }
        }

        /// <summary>
        /// Sets default values in a config object, then proceeds to search for sub-object to set defaults in
        /// </summary>
        public static void AddMissingCascade(this IConfig config)
        {
            config.AddMissing();


            // Foreach property in this that is iconfig, addmissingcascade
            var properties = config.GetType().GetProperties();

            foreach (PropertyInfo prop in properties)
            {
                object subObject = prop.GetValue(config);

                if (subObject is IConfig)
                {
                    (subObject as IConfig).AddMissingCascade();
                }
            }
        }


        /// <summary>
        /// Returns all differences between two config instances
        /// This only applies for properties and not fields
        /// </summary>
        public static bool IsChanged(this IConfig newConfig, IConfig oldConfig)
        {
            PropertyInfo[] props = newConfig.GetType().GetProperties();

            // Iterate through all properties and look for changes
            foreach (PropertyInfo p in props)
            {
                string name = p.Name;
                object oldValue = p.GetValue(oldConfig, null);
                object newValue = p.GetValue(newConfig, null);

                // Sub value is changed?
                if (IsChanged(newValue, oldValue))
                {
                    return true;
                }

            }

            return false;

        }

        /// <summary>
        /// A sub object in config is changed ? 
        /// </summary>
        private static bool IsChanged(object newObject, object oldObject)
        {
            // if newObject and oldObject are not equal null or not null, they are not the same
            if ((newObject == null) != (oldObject == null))
            {
                return true;
            }

            // Config object
            if (newObject is IConfig)
            {
                return (newObject as IConfig).IsChanged(oldObject as IConfig);
            }

            // Collection of objects
            else if (newObject is IEnumerable &&
                     newObject != null &&
                     oldObject != null &&
                     !(newObject is string))
            {
                IEnumerator oldEnumerator = (oldObject as IEnumerable).GetEnumerator();
                IEnumerator newEnumerator = (newObject as IEnumerable).GetEnumerator();

                // Iterate through all objects
                while (oldEnumerator.MoveNext() &&
                        newEnumerator.MoveNext())
                {
                    if (IsChanged(newEnumerator.Current,
                                  oldEnumerator.Current))
                    {
                        return true;
                    }
                }

                // Not equal length!
                if (oldEnumerator.MoveNext() ||
                    newEnumerator.MoveNext())
                {
                    return true;
                }
            }

            // Object is a simple IComparable
            else if (newObject is IComparable)
            {
                return !Equals(oldObject, newObject);
            }

            return false;
        }
    }
}
