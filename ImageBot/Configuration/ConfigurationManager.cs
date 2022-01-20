using Disboard;
using Disboard.Mastodon;
using Disboard.Mastodon.Enums;
using Microsoft.Extensions.Logging;
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
        private ILogger _logger;
        private Config _config;
        private MastodonClient _client = null;

        public string FileName { get; set; } = _defaultFileName;
        public AccessScope Scopes { get; set; } = AccessScope.Read | AccessScope.Write;


        public ConfigurationManager(ILogger logger, string instanceUrl)
        {
            _logger = logger;
            _client = new MastodonClient(instanceUrl);

            _config = new Config()
            {
                Instance = instanceUrl
            };
        }


        public async Task RegisterApplication(string applicationName)
        {
            _logger.LogDebug("Registering bot...");

            _config.ApplicationName = !string.IsNullOrEmpty(applicationName) ? applicationName : _defaultApplicationName;

            try
            {
                await _client.Apps.RegisterAsync(_config.ApplicationName, Constants.RedirectUriForClient, Scopes);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                _logger.LogError("Error: Could not connect to instance server.");
                throw;
            }
        }

        public string GetAuthorizationUrl()
        {
            return _client.Auth.AuthorizeUrl(Constants.RedirectUriForClient, Scopes);
        }

        public async Task GetAccessToken(string code)
        {
            _logger.LogDebug("Getting access token...");

            try
            {
                await _client.Auth.AccessTokenAsync(Constants.RedirectUriForClient, code);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                _logger.LogError("Error: Could not connect to instance server.");
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


        public static bool IsConfigured()
        {
            return IsConfigured(_defaultFileName);
        }

        public static bool IsConfigured(string filename)
        {
            return FileHelpers.CheckSerializedFileExists<Config>(filename);
        }

        public static Config LoadConfigFile()
        {
            return LoadConfigFile(_defaultFileName);
        }

        public static Config LoadConfigFile(string filename)
        {
            return FileHelpers.LoadSerializedFile<Config>(filename);
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
                _logger.LogError("Error: Failed to save configuration to file.");
                throw;
            }
        }
    }
}
