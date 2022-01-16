using Disboard;
using Disboard.Mastodon;
using Disboard.Mastodon.Enums;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ImageBot
{
    enum AcceptedInput
    {
        Empty,
        NoSpaces,
        Url
    }

    class Program
    {
        private static string _configFile = "config.json"; // TODO: rename? config.json credentials.json. credentials only save???
        // Add settings.json
        // stats.json
        // Unlimited retry

        private static Config _config;

        // # Client
        private static MastodonClient _client = null;
        private static AccessScope _scopes = AccessScope.Read | AccessScope.Write;


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DeleteConfigFile(); // TODO: remove.

            if (IsConfigured())
            {

            }
            else
            {
                LogWarning("Bot have not been configured.");
                SetupAsync().Wait();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static bool IsConfigured()
        {
            if (File.Exists(_configFile))
            {
                _config = LoadConfigFile();
                if (_config != null && _config.IsConfigured)
                {
                    return true;
                }
            }

            return false;
        }

        private static Config LoadConfigFile()
        {
            Config config = null;

            try
            {
                string json = File.ReadAllText(_configFile);
                config = JsonConvert.DeserializeObject<Config>(json);
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Could not open file.");
            }

            return config;
        }

        private static async Task SetupAsync()
        {
            Console.WriteLine("Starting initial setup. Follow the instructions to setup bot.");
            _config = new Config();

            // Instance Url
            Console.WriteLine();
            ///_config.Instance = GetInstanceUrl();

            // Application Name
            Console.WriteLine();
            ///_config.ApplicationName = GetApplicationName();


            // Register application
            _client = new MastodonClient(_config.Instance);
            await RegisterApplication();

            // Authorize Url
            AuthorizeBot();

            // Code
            string code = GetAuthorizationCode();

            // Access token
            await GetAccessToken(code);

            // Verify
            //_client.Apps.VerifyCredentialsAsync()

            // Save Credentials
            // ....

            Console.WriteLine("Done");

            // Save
            SaveConfigFile();
        }

        private static async Task GetAccessToken(string code)
        {
            Console.WriteLine("Getting access token...");

            bool error = true;
            ConsoleKey key;
            do
            {
                try
                {
                    await _client.Auth.AccessTokenAsync(Constants.RedirectUriForClient, code);
                    error = false;
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    LogError("Error: Could not connect to instance server.");
                }

                if (error == false)
                {
                    break;
                }

                Console.WriteLine("Try again? (Y/n): ");
                key = Console.ReadKey().Key;
                Console.WriteLine();

                if (key == ConsoleKey.N)
                {
                    Console.WriteLine("Exiting...");
                    Environment.Exit(1);
                }

            } while (true);
        }

        private static string GetAuthorizationCode()
        {
            string code = string.Empty;
            bool valid = false;
            do
            {
                Console.Write("Code: ");
                code = Console.ReadLine();

                Console.WriteLine("len: " + code.Length);
                if (!string.IsNullOrEmpty(code) && code.Length == 43)
                {
                    valid = true;
                }
                else
                {
                    Console.WriteLine("Invalid code.");
                }

            } while (!valid);

            return code;
        }

        private static void AuthorizeBot()
        {
            Console.WriteLine("Authorize bot");
            string url = string.Empty;

            try
            {
                 url = _client.Auth.AuthorizeUrl(Constants.RedirectUriForClient, _scopes);
                Process.Start(url); // Open in browser.
            }
            catch (System.ComponentModel.Win32Exception)
            {
                LogError("Error: Failed to open browser.");
            }

            Console.WriteLine("If the link doesn't automatically open, copy and paste this link in your browser and authorize the bot:");
            Console.WriteLine(url);
        }



        private static async Task RegisterApplication()
        {
            Console.WriteLine("Registering bot...");
            bool error = true;
            ConsoleKey key;
            do
            {
                try
                {
                    await _client.Apps.RegisterAsync(_config.ApplicationName, Constants.RedirectUriForClient, _scopes);
                    error = false;
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    LogError("Error: Could not connect to instance server.");
                }

                if (error == false)
                {
                    break;
                }

                Console.WriteLine("Try again? (Y/n): ");
                key = Console.ReadKey().Key;
                Console.WriteLine();

                if (key == ConsoleKey.N)
                {
                    Console.WriteLine("Exiting...");
                    Environment.Exit(1);
                }

            } while (true);
        }

        private static string GetInstanceUrl()
        {
            string input = string.Empty;
            bool valid = true;
            do
            {
                if (valid)
                {
                    Console.Write("Enter instance url (Example: mastodon.social): ");
                }
                else
                {
                    Console.Write("Invalid input. Enter application name (Default: ImageBot): ");
                }

                input = Console.ReadLine();


                // Format
                input = input.ToLower();
                input = input.Replace("http://", "");
                input = input.Replace("https://", "");

                // Validate
                if (!string.IsNullOrEmpty(input))
                {
                    // TODO: and regex valid.
                    valid = true;
                }
                else
                {
                    valid = false;
                }

            } while (!valid);


            return input;
        }

        private static string GetApplicationName()
        {
            string input = string.Empty;
            bool valid = true;
            do
            {
                if (valid)
                {
                    Console.Write("Enter application name (Default: ImageBot): ");
                }
                else
                {
                    Console.Write("Invalid input. Enter application name (Default: ImageBot): ");
                }

                input = Console.ReadLine();

                // Validate
                if (string.IsNullOrEmpty(input))
                {
                    valid = true;
                } // TODO: else if regex
                else
                {
                    valid = false;
                }

            } while (!valid);

            return input;
        }

        private static void DeleteConfigFile()
        {
            if (File.Exists(_configFile))
            {
                File.Delete(_configFile);
            }
        }

        private static void SaveConfigFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_config);
                File.WriteAllText(_configFile, json);
            }
            catch (IOException)
            {
                LogError("Error: Failed to save configuration to file.");
            }
        }

        private static void LogWarning(string message)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }

        private static void LogError(string message)
        {
            ConsoleColor temp = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = temp;
        }
    }

    class Config
    {
        public bool IsConfigured { get; set; }
        public string Instance { get; set; } = "mstdn.jp";
        public string ApplicationName { get; set; } = "ImageBot";
    }
}
