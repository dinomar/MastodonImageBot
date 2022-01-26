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
        private DateTime _nextPost;
        private Stats _stats;

        public Settings Settings { get { return _settings; } }

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

            InitializationAndChecks();
            await MainLoopAsync(() => !_cancelToken.IsCancellationRequested);
        }

        public async Task StartAsync()
        {
            InitializationAndChecks();
            await MainLoopAsync(() => true);
        }

        private void InitializationAndChecks()
        {
            VerifySettings();

            FileHelpers.CreateDirectoriesIfNotExist(new string[] { _settings.Folder1, _settings.Folder2 }, _logger);

            if (FileHelpers.IsDirectoryEmpty(_settings.Folder1) && FileHelpers.IsDirectoryEmpty(_settings.Folder2))
            {
                _logger.LogWarning("Image folders empty. No images found to post. Exiting program...");
                return;
            }

            _stats = Stats.Instance;

            ResetNextPostTimer();
        }


        private async Task MainLoopAsync(Func<bool> exitCondition)
        {
            _logger.LogDebug($"Waiting {_settings.Interval} minutes until next post.");

            do
            {
                if (DateTime.Now >= _nextPost)
                {
                    await UploadImage();
                    _logger.LogDebug($"Waiting {_settings.Interval} minutes until next post.");
                }

                await Task.Delay(1000);
            } while (exitCondition());
        }

        private void ResetNextPostTimer()
        {
            _nextPost = DateTime.Now + TimeSpan.FromMinutes(_settings.Interval);
        }

        private async Task UploadImage()
        {
            if (FileHelpers.IsDirectoryEmpty(_settings.CurrentFolder))
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

                _stats.IncrementPosts();

                _logger.LogInformation($"Successfully uploaded image. Posts made: {_stats?.Posts}");

                _stats.Save();
            }
            catch (System.Net.Http.HttpRequestException)
            {
                _logger.LogError("Network error: Failed to upload image.");
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
            finally
            {
                ResetNextPostTimer();
            }
            
            MoveFileToDepositFolder(file);
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
            Settings settings = new Settings();
            FileHelpers.SaveObjectToFile(_defaultSettingsFileName, settings);
        }

        private void SaveSettings()
        {
            FileHelpers.SaveObjectToFile(_defaultSettingsFileName, _settings);
        }

        public static bool SettingsFileExits()
        {
            return FileHelpers.SerializedFileExists<Settings>(_defaultSettingsFileName);
        }

        private Settings LoadSettingsFile()
        {
            return FileHelpers.LoadSerializedFile<Settings>(_defaultSettingsFileName);
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

        private void MoveFileToDepositFolder(string filename)
        {
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
    }
}
