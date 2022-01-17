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

            try
            {
                string json = File.ReadAllText(filename);
                temp = JsonConvert.DeserializeObject<T>(json);
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Could not open file.");
                throw;
            }

            return temp;
        }
    }
}
