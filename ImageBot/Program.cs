using Newtonsoft.Json;
using System;
using System.IO;

namespace ImageBot
{
    class Program
    {
        private static string _configFile = "settings.json";
        private static Config _config;

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            IsConfigured();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static bool IsConfigured()
        {
            if (File.Exists(_configFile))
            {
                _config = LoadConfigFile();
                if (_config != null && _config.IsConfigured)
                {
                    return true;
                }
            }

            return false;
        }

        private static Config LoadConfigFile()
        {
            Config config = null;

            try
            {
                string json = File.ReadAllText(_configFile);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error: Could not open file.");
            }

            return config;
        }

        private static void Setup()
        {

        }

        private static void DeleteConfigFile()
        {
            if (File.Exists(_configFile))
            {
                File.Delete(_configFile);
            }
        }
    }

    class Config
    {
        public bool IsConfigured { get; set; }
    }
}
