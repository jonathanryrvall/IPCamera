using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IPCamera.Model.Config
{
    /// <summary>
    /// A class that monitors an IConfig for changes in the configuration
    /// </summary>
    public class ConfigMonitor
    {
        public event EventHandler ConfigChanged;
        private IConfig config;

        /// <summary>
        /// Create a new config monitor
        /// </summary>
        public ConfigMonitor(IConfig config)
        {
            this.config = config;
            SetupEvents(config);
        }

        /// <summary>
        /// Setup events
        /// </summary>
        private void SetupEvents(object obj)
        {


            // Object has inotifypropertychanged
            if (obj is INotifyPropertyChanged)
            {
                (obj as INotifyPropertyChanged).PropertyChanged += PropertyChanged;

                // Search for any sub items that may implement any of theese interfaces


            }

            // Sub items
            if (obj is IConfig)
            {
                var properties = obj.GetType().GetProperties();

                foreach (PropertyInfo prop in properties)
                {
                    object subObject = prop.GetValue(obj);
                    SetupEvents(subObject);
                }
            }

            // Object has collectionchanged
            if (obj is INotifyCollectionChanged)
            {
                (obj as INotifyCollectionChanged).CollectionChanged += CollectionChanged;
            }

            // Object is Ienumerable, iterate through all items in it and setup events for them too
            if (obj is IEnumerable)
            {
                foreach (object item in obj as IEnumerable)
                {
                    SetupEvents(item);
                }
            }


        }

        /// <summary>
        /// A collection has been changed
        /// </summary>
        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConfigChanged?.Invoke(sender, e);

            // If new items have been added
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (object obj in e.NewItems)
                {
                    SetupEvents(obj);
                }
            }
        }

        /// <summary>
        /// A property has been changed
        /// </summary>
        private void PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ConfigChanged?.Invoke(sender, e);
        }





    }
}
