using Disboard.Mastodon;
using Disboard.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
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
        private Random _random = new Random();
        private CancellationToken _cancelToken;

        public string SettingsFileName { get; set; } = _defaultSettingsFileName;
        public Settings Settings { get { return _settings; } }

        public BotManager(ILogger logger, Credential credential)
        {
            _logger = logger;
            _client = new MastodonClient(credential);
        }

        // TODO: check _client not null
        // TODO: network error
        // TODO: filter images
        // TODO: CancelToken
        // TODO: change folder, save settings

        public async Task StartAsync(CancellationToken cancelToken)
        {
            _cancelToken = cancelToken;

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
            CreateDirectory(_settings.Folder1);
            CreateDirectory(_settings.Folder2);

            if (DirectoryEmpty(_settings.Folder1) && DirectoryEmpty(_settings.Folder2))
            {
                _logger.LogWarning("No images found. Exiting program...");
                return;
            }

            if (_settings.Interval <= 0) { _settings.Interval = 60; }

            await MainLoopAsync();
        }


        private async Task MainLoopAsync()
        {
            do
            {
                int delay = _settings.Interval * 60 * 1000;
                _logger.LogDebug($"Waiting {_settings.Interval} minutes until next post.");

                await Task.Delay(300000);

                await UploadImage();
            } while (!_cancelToken.IsCancellationRequested);
        }

        private async Task UploadImage()
        {
            if (DirectoryEmpty(_settings.CurrentFolder))
            {
                SwitchCurrentDirectories();
            }

            string file = GetRandomFile();
            //string filePath = Path.Combine(_settings.CurrentFolder, file);

            _logger.LogDebug($"Uploading '{file}'. Visibility: {_settings.Visibility}. IsSensitive: {_settings.IsSensitive}");

            var attachment = await _client.Media.CreateAsync(file);
            await _client.Statuses.UpdateAsync(
                status: "",
                inReplyToId: null,
                mediaIds: new List<long>() { attachment.Id },
                isSensitive: _settings.IsSensitive,
                spoilerText: null,
                visibility: Disboard.Mastodon.Enums.VisibilityType.Private); //_settings.Visibility Remove

            _logger.LogInformation($"Successfully uploaded image.");
            //_logger.LogDebug("Moving image to deposit folder.");
            MoveFile(file);
        }


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


        private string GetRandomFile()
        {
            string[] files = Directory.GetFiles(_settings.CurrentFolder);
            return files[_random.Next(0, files.Length)];
        }

        private void SwitchCurrentDirectories()
        {
            if (_settings.CurrentFolder == _settings.Folder1)
            {
                _settings.CurrentFolder = _settings.Folder2;
            }
            else
            {
                _settings.CurrentFolder = _settings.Folder1;
            }
        }

        private void MoveFile(string filename)
        {
            //string oldPath = Path.Combine(_settings.CurrentFolder, filename);
            if (File.Exists(filename))
            {
                string newPath = Path.Combine(_settings.DepositFolder, Path.GetFileName(filename));
                try
                {
                    File.Move(filename, newPath);
                }
                catch (IOException)
                {
                    _logger.LogError($"Failed to move file '{filename}'.");
                    throw;
                }
            }
            else
            {
                _logger.LogError($"Could not move file '{filename}'. File doesn't exist.");
            }
        }

        private bool DirectoryEmpty(string directory)
        {
            if (Directory.Exists(directory) && Directory.GetFiles(directory).Length == 0)
            {
                return true;
            }

            return false;
        }

        private void CreateDirectory(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    _logger.LogWarning($"'{directory}' folder doesn't exist.");
                    _logger.LogDebug($"Creating '{directory}' directory.");
                    Directory.CreateDirectory(directory);
                    _logger.LogDebug($"Created '{directory}' directory.");
                }
            }
            catch (IOException)
            {
                _logger.LogError($"Failed to create '{directory}' directory.");
                throw;
            }
        }
    }
}
