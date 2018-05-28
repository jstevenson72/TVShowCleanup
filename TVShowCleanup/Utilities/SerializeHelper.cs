using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace TVShowCleanup.Utilities
{
    public class SerializeHelper<T> where T : class, new()
    {
        public T ReadXMLSettings(string path)
        {
            try
            {
                // Read Settings from XML File
                if (File.Exists(path))
                {
                    using (var f = File.OpenRead(path))
                    {
                        var x = new XmlSerializer(typeof(T));

                        // Deserialize Saved CounterSketchOptions Class
                        var settingsClass = x.Deserialize(f) as T;

                        // Return Options
                        return settingsClass;
                    }
                }
                // Create Folder Structure
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            catch (Exception)
            {

            }

            // Return Default
            return new T();
        }

        public T ReadJSONSettings(string path)
        {
            try
            {
                // Read Settings from XML File
                if (File.Exists(path))
                {
                    using (var f = File.OpenRead(path))
                    {
                        var x = new DataContractJsonSerializer(typeof(T));

                        // Deserialize Saved Class
                        var settingsClass = x.ReadObject(f) as T;

                        // Return Options
                        if (settingsClass != null)
                        {
                            return settingsClass;
                        }
                    }
                }
                else
                {
                    // Create Folder Structure
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
            }
            catch (Exception)
            {
                //ignore
            }

            // Return Default
            return new T();
        }

        public List<T> ReadListFromJSON(string path)
        {
            if (path != null)
            {
                if (File.Exists(path))
                {
                    var jsonText = File.ReadAllText(path);
                    var items = JsonConvert.DeserializeObject<List<T>>(jsonText);
                    return items;
                }
            }

            return new List<T>();
        }

        public void WriteXMLSettings(string path, T classToSerialze)
        {
            using (var f = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                var x = new XmlSerializer(typeof(T));
                x.Serialize(f, classToSerialze);
            }
        }

        public void WriteJSONSettings(string path, T classToSerialze)
        {
            using (var f = File.Open(path, FileMode.Create, FileAccess.Write))
            {
                var jsonSerializer = new DataContractJsonSerializer(typeof(T));
                jsonSerializer.WriteObject(f, classToSerialze);
            }
        }

        public void WriteListToJSON(string path, List<T> classesToSerialize)
        {
            var jsonText = JsonConvert.SerializeObject(classesToSerialize, Formatting.Indented);
            if (File.Exists(path))
            {
                File.SetAttributes(path, FileAttributes.Normal);
                File.Delete(path);
            }
            File.AppendAllText(path, jsonText);
        }
    }
}