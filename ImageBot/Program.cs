using ImageBot.Bot;
using ImageBot.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;

namespace ImageBot
{
    class Program
    {
        private static CancellationTokenSource _cancelTokenSource;

        static async Task Main(string[] args)
        {
            PrintBanner();
            Console.CancelKeyPress += Console_CancelKeyPress;

            // Check configured
            if (!ConfigurationManager.IsConfigured())
            {
                // Setup
                await SetupAsync();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }


            // Load config
            Config config = null;
            try
            {
                config = ConfigurationManager.LoadConfigFile();
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error: Failed to load config file. Message: {ex.Message}");
                Environment.Exit(1);
            }


            // Load setting
            Settings settings = null;
            if (!BotManager.SettingsFileExits())
            {
                Console.WriteLine("Settings file doesn't exist. Rerun setup to fix. Exiting program...");
                WaitForExitWithError();
            }
            settings = BotManager.LoadSettingsFile();


            // Check folders
            if (!Directory.Exists(settings.Folder1))
            {
                Console.WriteLine($"Image folder '{settings.Folder1}' does not exist. Exiting program...");
                WaitForExitWithError();
            }

            if (!Directory.Exists(settings.Folder2))
            {
                Console.WriteLine($"Image folder '{settings.Folder2}' does not exist. Exiting program...");
                WaitForExitWithError();
            }

            // Check for images
            if (FileHelpers.IsDirectoryEmpty(settings.Folder1) && FileHelpers.IsDirectoryEmpty(settings.Folder2))
            {
                Console.WriteLine("No images in folders. Exiting program...");
                WaitForExitWithError();
            }


            // Start bot
            Console.WriteLine("Starting bot...");
            Console.WriteLine("Press Ctrl + C to Stop.");
            BotManager bot = new BotManager(config.Credential, settings);
            _cancelTokenSource = new CancellationTokenSource();
            
            do
            {
                Console.WriteLine($"Waiting {bot.Settings.Interval} minutes until next post.");
                await bot.WaitForNextPost(_cancelTokenSource.Token);

                if (_cancelTokenSource.Token.IsCancellationRequested) { break; }


                try
                {
                    Console.WriteLine($"Uploading '{bot.NextImage}'. Visibility: {bot.Settings.Visibility}. IsSensitive: {bot.Settings.IsSensitive}");
                    await bot.PostNextImage();
                    Console.WriteLine("Successfully uploaded image.");
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    Console.WriteLine("Network error: Failed to upload image.");
                }
                catch (Disboard.Exceptions.DisboardException ex)
                {
                    Console.WriteLine($"Server error: {ex.Message}");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"File error: {ex.Message}");
                }
            } while (!_cancelTokenSource.Token.IsCancellationRequested);
        }

        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _cancelTokenSource.Cancel();
        }


        private static async Task SetupAsync()
        {
            Console.WriteLine("Starting initial setup. Follow the instructions to setup bot.");

            // Create default image folders.
            try
            {
                Console.WriteLine("Creating default image folders");
                CreateDefaultImageFolders();
                Console.WriteLine("Default image folders created.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to create default image folders. Error: {ex.Message}. Exiting program...");
                WaitForExitWithError();
            }

            // Create default settings file.
            try
            {
                Console.WriteLine("Creating default setting file.");
                BotManager.CreateDefaultSettingsFile();
                Console.WriteLine("Default setting file created.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Failed to create default settings file. Error: {ex.Message}. Exiting program...");
                WaitForExitWithError();
            }

            // Get instance url.
            string instanceUrl = GetInstanceUrl();

            // Get appliction name
            string applicationName = GetApplicationName();

            // Create Configuration manager.
            ConfigurationManager manager = new ConfigurationManager(instanceUrl);

            // Register application
            try
            {
                Console.WriteLine("Registering bot...");
                await manager.RegisterApplication(applicationName);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Console.WriteLine("Network error: Could not connect to instance server.");
                WaitForExitWithError();
            }
            catch (Disboard.Exceptions.DisboardException ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                WaitForExitWithError();
            }

            // Get Auth Url.
            string authUrl = manager.GetAuthorizationUrl();

            // Open auth url in browser.
            try
            {
                Console.WriteLine("Opening link in browser.");
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = authUrl;
                process.Start();
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Console.WriteLine("Failed to open browser.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open browser. Error: {ex.Message}");
            }

            Console.WriteLine("If the link doesn't automatically open, copy and paste this link in your browser and authorize the bot:");
            Console.WriteLine(authUrl);

            // Get auth code from user.
            string code = GetAuthorizationCode();

            // Get access token.
            try
            {
                Console.WriteLine("Getting access token...");
                await manager.GetAccessToken(code);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Console.WriteLine("Network error: Could not connect to instance server.");
                WaitForExitWithError();
            }
            catch (Disboard.Exceptions.DisboardException ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                WaitForExitWithError();
            }

            // Verify configuration.
            if (manager.Verify())
            {
                try
                {
                    manager.SaveToFile();
                }
                catch (Exception)
                {
                    Console.WriteLine("Error: Failed to save configuration to file.");
                    WaitForExitWithError();
                }
                
                Console.WriteLine("Setup complete! You can now copy your images into the 'images1' folder. And edit the 'settings.json' file to change the time between posts, post visibility, and whether or not to mark the post as sensitive. When you are ready, run this application again.");
            }
            else
            {
                Console.WriteLine("Setup failed!");
            }
        }


        private static void CreateDefaultImageFolders()
        {
            Settings settings = new Settings();
            FileHelpers.CreateDirectoriesIfNotExist(new string[] { settings.Folder1, settings.Folder2 });
        }

        private static void WaitForExitWithError()
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            Environment.Exit(1);
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
