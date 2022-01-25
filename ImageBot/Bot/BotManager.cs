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
        private ILogger _logger;
        private MastodonClient _client;
        private Settings _settings;
        private Random _random = new Random();
        private CancellationToken _cancelToken;

        public string SettingsFileName { get; set; } = _defaultSettingsFileName;
        public Settings Settings { get { return _settings; } }
        private int Delay { get { return _settings.Interval * 60 * 1000; } }

        public BotManager(ILogger logger, Credential credential)
        {
            if (logger == null) { throw new ArgumentNullException(paramName: nameof(logger)); }
            if (credential == null) { throw new ArgumentNullException(paramName: nameof(credential)); }

            _logger = logger;
            _client = new MastodonClient(credential);
        }
        

        public async Task StartAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null) { throw new ArgumentNullException(paramName: nameof(cancelToken)); }
            _cancelToken = cancelToken;

            VerifySettings();
          
            VerifyOrCreateDirectory(_settings.Folder1);
            VerifyOrCreateDirectory(_settings.Folder2);

            if (DirectoryEmpty(_settings.Folder1) && DirectoryEmpty(_settings.Folder2))
            {
                _logger.LogWarning("Image folders empty. No images found to post. Exiting program...");
                return;
            }

            await MainLoopAsync(() => !_cancelToken.IsCancellationRequested);
        }

        public async Task StartAsync()
        {
            VerifySettings();

            VerifyOrCreateDirectory(_settings.Folder1);
            VerifyOrCreateDirectory(_settings.Folder2);

            if (DirectoryEmpty(_settings.Folder1) && DirectoryEmpty(_settings.Folder2))
            {
                _logger.LogWarning("Image folders empty. No images found to post. Exiting program...");
                return;
            }

            await MainLoopAsync(() => true);
        }


        private async Task MainLoopAsync(Func<bool> exitCondition)
        {
            do
            {
                _logger.LogDebug($"Waiting {_settings.Interval} minutes until next post.");

                await Task.Delay(Delay);

                await UploadImage();
            } while (exitCondition());
        }

        private async Task UploadImage()
        {
            if (DirectoryEmpty(_settings.CurrentFolder))
            {
                SwitchCurrentDirectories();
            }

            string file = GetRandomFile();

            _logger.LogDebug($"Uploading '{file}'. Visibility: {_settings.Visibility}. IsSensitive: {_settings.IsSensitive}");

            try
            {
                var attachment = await _client.Media.CreateAsync(file);
                await _client.Statuses.UpdateAsync(
                    status: "",
                    inReplyToId: null,
                    mediaIds: new List<long>() { attachment.Id },
                    isSensitive: _settings.IsSensitive,
                    spoilerText: null,
                    visibility: _settings.Visibility);

                Stats stats = StatsManager.GetStats();
                stats.IncrementPosts();

                _logger.LogInformation($"Successfully uploaded image. Posts made: {stats?.Posts}");

                StatsManager.SaveStats(stats);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                _logger.LogError("Network error: Failed to image.");
            }
            catch (Disboard.Exceptions.DisboardException ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                if (ex.Message == "Too many requests")
                {
                    _logger.LogWarning("Increasing delay between posts by 5 minutes.");
                    _settings.Interval += 5;
                }
            }
            
            MoveFile(file);
        }


        private void VerifySettings()
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

            if (_settings.Interval <= 0) { _settings.Interval = 60; }
        }


        public static void CreateDefaultSettingsFile()
        {
            CreateDefaultSettingsFile(_defaultSettingsFileName);
        }

        public static void CreateDefaultSettingsFile(string filename)
        {
            Settings settings = new Settings();
            FileHelpers.SaveObjectToFile(filename, settings);
        }

        private void SaveSettings()
        {
            FileHelpers.SaveObjectToFile(_defaultSettingsFileName, _settings);
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

            SaveSettings();
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

        private void VerifyOrCreateDirectory(string directory)
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
