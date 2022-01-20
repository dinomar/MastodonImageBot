using Disboard.Mastodon;
using Disboard.Models;
using Microsoft.Extensions.Logging;
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
        private ILogger _logger;
        private MastodonClient _client;
        private Settings _settings;

        public string SettingsFileName { get; set; } = _defaultSettingsFileName;
        public Settings Settings { get { return _settings; } }

        public BotManager(ILogger logger, Credential credential)
        {
            _logger = logger;
            _client = new MastodonClient(credential);
        }

        public async Task StartAsync()
        {
            if (!SettingsFileExits())
            {
                _logger.LogWarning("Settings file doesn't exist.");
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
            _logger.LogDebug("Waiting 60 minutes until next post.");

            Console.WriteLine("Main loop");
            var attachment = await _client.Media.CreateAsync(@"C:\Users\dinom\Pictures\img.jpg");
            await _client.Statuses.UpdateAsync(
                status: "",
                inReplyToId: null,
                mediaIds: new List<long>() { attachment.Id },
                isSensitive: true,
                spoilerText: null,
                visibility: Disboard.Mastodon.Enums.VisibilityType.Private);

            Console.WriteLine("Post");
        }

        // getRandomImage
        // PostImage
        // Move Image
        // UpdateStats


        public static void CreateDefaultSettingsFile()
        {
            CreateDefaultSettingsFile(_defaultSettingsFileName);
        }

        public static void CreateDefaultSettingsFile(string filename)
        {
            Settings settings = new Settings();

            try
            {
                string json = JsonConvert.SerializeObject(settings);
                File.WriteAllText(filename, json);
            }
            catch (IOException)
            {
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
