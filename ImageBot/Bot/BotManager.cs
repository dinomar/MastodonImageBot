using Disboard.Mastodon;
using Disboard.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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


        public void Start()
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


        }


        public void CreateDefaultSettingsFile()
        {
            Console.WriteLine("Creating new settings file.");
            Settings settings = new Settings();
            string json = JsonConvert.SerializeObject(settings);

            try
            {
                File.WriteAllText(SettingsFileName, json);
                Console.WriteLine("New settings file created.");
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Could not create settings file.");
                throw;
            }
        }

        public bool SettingsFileExits()
        {
            return FileHelpers.CheckSerializedFileExists<Settings>(SettingsFileName);
        }

        private Settings LoadSettingsFile()
        {
            return FileHelpers.LoadSerializedFile<Settings>(SettingsFileName);
        }
    }
}
