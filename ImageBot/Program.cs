using Disboard;
using Disboard.Mastodon;
using Disboard.Mastodon.Enums;
using ImageBot.Bot;
using ImageBot.Configuration;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static ImageBot.ConsoleHelpers;

namespace ImageBot
{
    class Program
    {
        //private static string _configFile = "config.json"; // TODO: rename? config.json credentials.json. credentials only save???
        // Add settings.json
        // stats.json
        // TODO: argument null
        // logger


        static void Main(string[] args)
        {
            PrintBanner();

            //ConfigurationManager.DeleteConfigFile(); // TODO: remove.

            try
            {
                if (!BotManager.SettingsFileExits())
                {
                    PrintWarning("Settings file doesn't exist.");
                    BotManager.CreateDefaultSettingsFile();
                }
            }
            catch (IOException)
            {
                Environment.Exit(1);
            }
            


            if (ConfigurationManager.IsConfigured())
            {
                Console.WriteLine("Starting bot...");
                Config config = ConfigurationManager.LoadConfigFile();
                BotManager bot = new BotManager(config.Credential);
                bot.StartAsync().Wait();
            }
            else
            {
                PrintWarning("Bot have not been configured.");
                SetupAsync().Wait();
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        

        // # Setup
        private static async Task SetupAsync()
        {
            Console.WriteLine("Starting initial setup. Follow the instructions to setup bot.");

            // Instance Url
            Console.WriteLine();
            string instanceUrl = GetInstanceUrl();
            ///string instanceUrl = "mstdn.jp";


            // Create Manager
            ConfigurationManager manager = new ConfigurationManager(instanceUrl);
            manager.Log += Manager_Log;
            manager.LogWarning += Manager_LogWarning;
            manager.LogError += Manager_LogError;


            // Get Application Name
            Console.WriteLine();
            string applicationName = GetApplicationName();
            ///string applicationName = "ImageBot";

            // Register application
            try
            {
                await manager.RegisterApplication(applicationName);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Environment.Exit(1);
            }


            // Get Auth Url
            string authUrl = manager.GetAuthorizationUrl();

            // Open in browser
            try
            {
                Process.Start(authUrl);
            }
            catch (System.ComponentModel.Win32Exception)
            {
                PrintError("Failed to open browser.");
            }

            Console.WriteLine("If the link doesn't automatically open, copy and paste this link in your browser and authorize the bot:");
            Console.WriteLine(authUrl);


            // Auth Code
            string code = GetAuthorizationCode();

            // Access token
            try
            {
                await manager.GetAccessToken(code);
            }
            catch (System.Net.Http.HttpRequestException)
            {
                Environment.Exit(1);
            }

            // Verify
            if (manager.Verify())
            {
                manager.SaveToFile();
                PrintSuccess("Setup complete! Edit the 'settings.json' file to your requirements then run this application again.");
                Environment.Exit(0);
            }
            else
            {
                PrintError("Setup failed!");
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


        // # Logging
        private static void Manager_Log(object sender, LogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private static void Manager_LogWarning(object sender, LogEventArgs e)
        {
            PrintWarning(e.Message);
        }

        private static void Manager_LogError(object sender, LogEventArgs e)
        {
            PrintError(e.Message);
        }
    }
}
