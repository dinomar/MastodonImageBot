using Disboard.Mastodon;
using Disboard.Models;
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
        private MastodonClient _client;
        private Settings _settings;
        private Random _random = new Random();
        private DateTime _nextPost;

        public Settings Settings { get => _settings; }
        public string NextImage { get; private set; } = String.Empty;

        public BotManager(Credential credential, Settings settings)
        {
            if (credential == null) { throw new ArgumentNullException(paramName: nameof(credential)); }
            if (settings == null) { throw new ArgumentNullException(paramName: nameof(settings)); }

            _client = new MastodonClient(credential);
            _settings = settings;
            SetNextImage();
        }
        
        public async Task PostNextImage()
        {
            var attachment = await _client.Media.CreateAsync(NextImage);
            await _client.Statuses.UpdateAsync(
                status: String.Empty,
                inReplyToId: null,
                mediaIds: new List<long>() { attachment.Id },
                isSensitive: _settings.IsSensitive,
                spoilerText: null,
                visibility: _settings.Visibility);

            MoveFileToDepositFolder(NextImage);
            SetNextImage();
        }

        public async Task WaitForNextPost(CancellationToken cancellationToken)
        {
            _nextPost = DateTime.Now + TimeSpan.FromMinutes(_settings.Interval);

            do
            {
                await Task.Delay(1000);
            } while (!cancellationToken.IsCancellationRequested && _nextPost > DateTime.Now);
        }

        private void SetNextImage()
        {
            if (FileHelpers.IsDirectoryEmpty(_settings.CurrentFolder)) { SwitchCurrentDirectories(); }
            NextImage = GetRandomFile();
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

        public static Settings LoadSettingsFile()
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
            string newPath = Path.Combine(_settings.DepositFolder, Path.GetFileName(filename));
            File.Move(filename, newPath);
        }
    }
}
