using Microsoft.Extensions.Logging;
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

        public static T CreateFileIfNotExists<T>(string filename, T objectToCreate) where T : class
        {

            if (!SerializedFileExists<T>(filename))
            {
                SaveObjectToFile(filename, objectToCreate);
            }

            return objectToCreate;
        }

        public static T CreateNewJsonFile<T>(string filename, T objectToCreate) where T : class
        {
            SaveObjectToFile(filename, objectToCreate);
            return objectToCreate;
        }

        public static void SaveObjectToFile(string filename, object objectToSave)
        {
            string json = JsonConvert.SerializeObject(objectToSave);
            File.WriteAllText(filename, json);
        }

        public static void CreateDirectoriesIfNotExist(string[] directories, ILogger logger = null)
        {
            if (directories == null) { throw new ArgumentNullException(paramName: nameof(directories)); }

            foreach (string dir in directories)
            {
                CreateDirectoryIfNotExist(dir, logger);
            }
        }

        public static void CreateDirectoryIfNotExist(string directory, ILogger logger = null)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    logger?.LogWarning($"'{directory}' folder doesn't exist.");
                    logger?.LogDebug($"Creating '{directory}' directory.");
                    Directory.CreateDirectory(directory);
                    logger?.LogDebug($"Created '{directory}' directory.");
                }
            }
            catch (IOException)
            {
                logger?.LogError($"Failed to create '{directory}' directory.");
                throw;
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
