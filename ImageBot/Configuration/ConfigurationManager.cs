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
        private Config _config;
        private MastodonClient _client = null;

        public AccessScope Scopes { get; set; } = AccessScope.Read | AccessScope.Write;


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
            _config.ApplicationName = !string.IsNullOrEmpty(applicationName) ? applicationName : _defaultApplicationName;
            await _client.Apps.RegisterAsync(_config.ApplicationName, Constants.RedirectUriForClient, Scopes);
        }

        public string GetAuthorizationUrl()
        {
            return _client.Auth.AuthorizeUrl(Constants.RedirectUriForClient, Scopes);
        }

        public async Task GetAccessToken(string code)
        {
            await _client.Auth.AccessTokenAsync(Constants.RedirectUriForClient, code);
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
            return FileHelpers.SerializedFileExists<Config>(_defaultFileName);
        }

        public static Config LoadConfigFile()
        {
            return FileHelpers.LoadSerializedFile<Config>(_defaultFileName);
        }

        public void SaveToFile()
        {
            _config.Credential = _client.Credential;
            FileHelpers.SaveObjectToFile(_defaultFileName, _config);
        }
    }
}
