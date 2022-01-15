using Disboard;
using Disboard.Mastodon;
using Disboard.Mastodon.Enums;
using Newtonsoft.Json;
using System;
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
        private static string _configFile = "settings.json";
        private static Config _config;

        // # Client
        private static MastodonClient _client = null;
        private static AccessScope _scopes = AccessScope.Read | AccessScope.Write;


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            DeleteConfigFile();

            if (IsConfigured())
            {

            }
            else
            {
                Console.WriteLine("Bot have not been configured.");
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
            Console.WriteLine("Starting initial setup. Follow the instructions to setup the bot.");
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

            // Auth user


            Console.WriteLine("registered");
            Console.WriteLine(_client.ClientSecret);

            try
            {
                string json = JsonConvert.SerializeObject(_config);
                File.WriteAllText(_configFile, json);
            }
            catch (IOException)
            {
                Console.WriteLine("Error: Failed to save configuration to file.");
            }
        }

        private static async Task RegisterApplication()
        {
            int retry = 3;
            bool errors = true;
            do
            {
                try
                {
                    if (retry < 3)
                    {
                        Console.WriteLine("Retrying...");
                    }

                    await _client.Apps.RegisterAsync(_config.ApplicationName, Constants.RedirectUriForClient, _scopes);
                    errors = false;

                }
                catch (System.Net.Http.HttpRequestException)
                {
                    Console.WriteLine("Error: Could not connect to instance server.");
                    retry -= 1;

                    // Try again
                    if (retry > 0)
                    {
                        ConsoleKey key;
                        do
                        {
                            Console.WriteLine("Try again? (Y/n): ");
                            key = Console.ReadKey().Key;
                            Console.WriteLine();

                            if (key == ConsoleKey.Y)
                            {
                                break;
                            }
                            else if (key == ConsoleKey.N)
                            {
                                // TODO: Exit
                                Console.WriteLine("Exiting.");
                            }

                        } while (true);
                    }
                }
            } while (errors && retry > 0);

            if (retry == 0)
            {
                // TODO: exit
                Console.WriteLine("Exiting.");
            }
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
    }

    class Config
    {
        public bool IsConfigured { get; set; }
        public string Instance { get; set; } = "mstdn.jp";
        public string ApplicationName { get; set; } = "ImageBot";
    }
}
