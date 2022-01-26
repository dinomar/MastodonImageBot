using Disboard;
using Disboard.Mastodon;
using Disboard.Mastodon.Enums;
using ImageBot.Bot;
using ImageBot.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ColorConsoleLogger;
using System.Threading;

namespace ImageBot
{
    class Program
    {
        // TODO: filter images, image in dir
        // TODO: Readme
        // TODO: Test | win linux

        private static ILoggerFactory _loggerFactory;
        private static ILogger _logger;

        static void Main(string[] args)
        {
            PrintBanner();

            // Logging
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddColorConsoleLogger(c =>
                {
                    c.IncludeNamePrefix = false;
                });
            });
            _logger = _loggerFactory.CreateLogger<Program>();


            try
            {
                CheckSettingsFileExists();
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }


            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            if (ConfigurationManager.IsConfigured())
            {
                _logger.LogDebug("Starting bot...");
                _logger.LogDebug("Press 'Esc' to exit program.");

                Config config = ConfigurationManager.LoadConfigFile();
                BotManager bot = new BotManager(_loggerFactory.CreateLogger<BotManager>(), config.Credential);
                Task botTask = null;

                try
                {
                    botTask = bot.StartAsync(cancelTokenSource.Token);
                    
                    do
                    {
                        if (Console.ReadKey().Key == ConsoleKey.Escape)
                        {
                            cancelTokenSource.Cancel();
                            botTask.Wait();
                            break;
                        }
                    } while (true);
                }
                catch (Exception)
                {
                    Environment.Exit(1);
                }
            }
            else
            {
                _logger.LogWarning("Bot have not been configured.");
                // Run Setup
                SetupAsync().Wait();
            }
        }


        // # Setup
        private static async Task SetupAsync()
        {
            _logger.LogDebug("Starting initial setup. Follow the instructions to setup bot.");

            try
            {
                // Create default image folders
                CreateDefaultImageFolders();

                // Create default settings file
                CreateDefaultSettingsFile();

                // Instance Url
                string instanceUrl = GetInstanceUrl();

                // Create Manager
                ConfigurationManager manager = new ConfigurationManager(_loggerFactory.CreateLogger<ConfigurationManager>(), instanceUrl);

                // Get Application Name
                string applicationName = GetApplicationName();

                // Register application
                await manager.RegisterApplication(applicationName);


                // Get Auth Url
                string authUrl = manager.GetAuthorizationUrl();

                // Open in browser
                try
                {
                    Process.Start(authUrl);
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    _logger.LogError("Failed to open browser.");
                }

                _logger.LogDebug("If the link doesn't automatically open, copy and paste this link in your browser and authorize the bot:");
                _logger.LogDebug(authUrl);


                // Auth Code
                string code = GetAuthorizationCode();

                // Access token
                await manager.GetAccessToken(code);

                // Verify
                if (manager.Verify())
                {
                    manager.SaveToFile();
                    _logger.LogInformation("Setup complete! You can now copy your images into the 'images1' folder. And edit the 'settings.json' file to meet your requirements. When your done, run this application again.");
                    Environment.Exit(0);
                }
                else
                {
                    _logger.LogError("Setup failed!");
                }
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }
        }


        // # User Input
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
                
                if (!string.IsNullOrEmpty(input) && Regex.Match(input, @".+\..+").Success)
                {
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
                }
                else if (Regex.Match(input, @"[\w-]+").Success)
                {
                    valid = true;
                }
                else
                {
                    valid = false;
                }

            } while (!valid);

            return input;
        }

        private static string GetAuthorizationCode()
        {
            string code = string.Empty;
            bool valid = false;
            do
            {
                Console.Write("Code: ");
                code = Console.ReadLine();

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

        private static void CreateDefaultImageFolders()
        {
            try
            {
                _logger.LogDebug("Creating default image folders.");
                Settings settings = new Settings();
                FileHelpers.CreateDirectoriesIfNotExist(new string[] { settings.Folder1, settings.Folder2 }, _logger);
            }
            catch (IOException ex)
            {
                _logger.LogError($"Failed to create directories. Error: {ex.Message}");
                throw;
            }
        }

        private static void CreateDefaultSettingsFile()
        {
            try
            {
                BotManager.CreateDefaultSettingsFile();
                _logger.LogDebug("Successfully created new settings file.");
            }
            catch (IOException ex)
            {
                _logger.LogError($"Failed to create default settings file. Error: {ex.Message}");
                throw;
            }
        }

        private static void CheckSettingsFileExists()
        {
            try
            {
                if (!BotManager.SettingsFileExits())
                {
                    _logger.LogWarning("Settings file doesn't exist.");

                    try
                    {
                        BotManager.CreateDefaultSettingsFile();
                        _logger.LogInformation("Successfully created new settings file.");
                    }
                    catch (IOException)
                    {
                        _logger.LogError("Failed to create new settings file.");
                        throw;
                    }
                }
            }
            catch (IOException ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }
        }


        // # Console
        private static void PrintBanner()
        {
            Console.WriteLine("#################################################");
            Console.WriteLine("###                                           ###");
            Console.WriteLine("###  MastodonImageBot                         ###");
            Console.WriteLine("###  by dinomar                               ###");
            Console.WriteLine("###  https://github/dinomar/MastodonImageBot  ###");
            Console.WriteLine("###                                           ###");
            Console.WriteLine("#################################################");
            Console.WriteLine();
        }
    }
}
