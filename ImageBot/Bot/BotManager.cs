using Disboard.Mastodon;
using Disboard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageBot.Bot
{
    class BotManager
    {
        private static readonly string _defaultSettingsFileName = "settings.json";
        private static readonly string _defaultStatsFileName = "stats.json";
        private MastodonClient _client;
        private Settings _settings;

        public string SettingsFileName { get; set; } = _defaultSettingsFileName;
        public Settings Settings { get { return _settings; } }

        public BotManager(Credential credential)
        {
            _client = new MastodonClient(credential);
        }

        public async Task StartAsync()
        {
            if (!SettingsFileExits())
            {
                Console.WriteLine("Settings file doesn't exist.");
                CreateDefaultSettingsFile();
            }
            else
            {
                _settings = LoadSettingsFile();
            }

            // Verify settings
            // check folders exits
            // check images in folders

            // check _client not null
            // main loop
            await MainLoopAsync();
        }

        private async Task MainLoopAsync()
        {
            Console.WriteLine("Main loop");
            var attachment = await _client.Media.CreateAsync(@"C:\Users\dinom\Pictures\img.jpg");
            Console.WriteLine("");
        }


        public static void CreateDefaultSettingsFile()
        {
            CreateDefaultSettingsFile(_defaultSettingsFileName);
        }

        public static void CreateDefaultSettingsFile(string filename)
        {
            Console.WriteLine("Creating new settings file.");
            Settings settings = new Settings();
            string json = JsonConvert.SerializeObject(settings);

            try
            {
                File.WriteAllText(filename, json);
                Console.WriteLine("New settings file created.");
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Could not create settings file.");
                throw;
            }
        }

        public static bool SettingsFileExits()
        {
            return SettingsFileExits(_defaultSettingsFileName);
        }

        public static bool SettingsFileExits(string filename)
        {
            return FileHelpers.CheckSerializedFileExists<Settings>(filename);
        }

        private Settings LoadSettingsFile()
        {
            return FileHelpers.LoadSerializedFile<Settings>(SettingsFileName);
        }
    }
}
