using Disboard;
using Disboard.Mastodon;
using Disboard.Mastodon.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ImageBot.Configuration
{
    class ConfigurationManager
    {
        private static readonly string _defaultFileName = "config.json";
        private static readonly string _defaultApplicationName = "ImageBot";
        private Config _config;
        private MastodonClient _client = null;

        public string FileName { get; set; } = _defaultFileName;
        public AccessScope Scopes { get; set; } = AccessScope.Read | AccessScope.Write;


        public event EventHandler<LogEventArgs> Log;
        public event EventHandler<LogEventArgs> LogWarning;
        public event EventHandler<LogEventArgs> LogError;


        public ConfigurationManager(string instanceUrl)
        {
            _client = new MastodonClient(instanceUrl);

            _config = new Config()
            {
                Instance = instanceUrl
            };
        }


        public async Task RegisterApplication(string applicationName)
        {
            OnLog("Registering bot...");

            _config.ApplicationName = !string.IsNullOrEmpty(applicationName) ? applicationName : _defaultApplicationName;

            try
            {
                await _client.Apps.RegisterAsync(_config.ApplicationName, Constants.RedirectUriForClient, Scopes);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnLogError("Error: Could not connect to instance server.");
                throw;
            }
        }

        public string GetAuthorizationUrl()
        {
            return _client.Auth.AuthorizeUrl(Constants.RedirectUriForClient, Scopes);
        }

        public async Task GetAccessToken(string code)
        {
            OnLog("Getting access token...");

            try
            {
                await _client.Auth.AccessTokenAsync(Constants.RedirectUriForClient, code);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                OnLogError("Error: Could not connect to instance server.");
                throw;
            }
        }

        public bool Verify()
        {
            if (!string.IsNullOrEmpty(_client.AccessToken) &&
                !string.IsNullOrEmpty(_client.ClientId) &&
                !string.IsNullOrEmpty(_client.ClientSecret))
            {
                return true;
            }

            return false;
        }


        public static bool IsConfigured() // TODO
        {
            return IsConfigured(_defaultFileName);
        }

        public static bool IsConfigured(string filename) // TODO
        {
            if (File.Exists(filename))
            {
                Config _config = LoadConfigFile(filename);
                if (_config != null && _config.IsConfigured)
                {
                    return true;
                }
            }

            return false;
        }

        public static Config LoadConfigFile() // TODO
        {
            return LoadConfigFile(_defaultFileName);
        }

        public static Config LoadConfigFile(string filename) // TODO
        {
            Config config = null;

            try
            {
                string json = File.ReadAllText(filename);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Could not open file.");
                throw;
            }

            return config;
        }

        public static void DeleteConfigFile()
        {
            DeleteConfigFile(_defaultFileName);
        }

        public static void DeleteConfigFile(string filename)
        {
            if (File.Exists(filename))
            {
                File.Delete(filename);
            }
        }

        public void SaveToFile()
        {
            _config.Credential = _client.Credential;

            try
            {
                string json = JsonConvert.SerializeObject(_config);
                File.WriteAllText(FileName, json);
            }
            catch (IOException)
            {
                OnLogError("Error: Failed to save configuration to file.");
                throw;
            }
        }

        // # Event Handlers
        private void OnLog(string message)
        {
            Log?.Invoke(this, new LogEventArgs(message));
        }

        private void OnLogWarning(string message)
        {
            LogWarning?.Invoke(this, new LogEventArgs(message));
        }

        private void OnLogError(string message)
        {
            LogError?.Invoke(this, new LogEventArgs(message));
        }
    }
}
