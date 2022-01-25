using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageBot
{
    static class FileHelpers
    {
        public static bool CheckSerializedFileExists<T>(string filename) where T : class
        {
            if (File.Exists(filename))
            {
                T temp = LoadSerializedFile<T>(filename);
                if (temp != null)
                {
                    return true;
                }
            }

            return false;
        }

        public static T LoadSerializedFile<T>(string filename) where T : class
        {
            T temp = null;

            string json = File.ReadAllText(filename);
            temp = JsonConvert.DeserializeObject<T>(json);

            return temp;
        }

        public static T CreateFileIfNotExists<T>(string filename) where T : class
        {
            T newObject = default(T);
            if (!CheckSerializedFileExists<T>(filename))
            {
                SaveObjectToFile(filename, newObject);
            }

            return newObject;
        }

        public static T CreateNewJsonFile<T>(string filename) where T : class
        {
            T newObject = default(T);
            SaveObjectToFile(filename, newObject);
            return newObject;
        }

        public static void SaveObjectToFile(string filename, object objectToSave)
        {
            string json = JsonConvert.SerializeObject(objectToSave);
            File.WriteAllText(filename, json);
        }
    }
}
