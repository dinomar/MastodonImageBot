using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageBot
{
    static class FileHelpers
    {
        public static bool SerializedFileExists<T>(string filename) where T : class
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

        public static void SaveObjectToFile(string filename, object objectToSave)
        {
            string json = JsonConvert.SerializeObject(objectToSave, Formatting.Indented);
            File.WriteAllText(filename, json);
        }

        public static void CreateDirectoriesIfNotExist(string[] directories)
        {
            if (directories == null) { throw new ArgumentNullException(paramName: nameof(directories)); }

            foreach (string dir in directories)
            {
                CreateDirectoryIfNotExist(dir);
            }
        }

        public static void CreateDirectoryIfNotExist(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static bool IsDirectoryEmpty(string directory)
        {
            if (Directory.Exists(directory) && Directory.GetFiles(directory).Length == 0)
            {
                return true;
            }

            return false;
        }
    }
}
