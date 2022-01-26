# MastodonImageBot

## Instructions

Download and install [.NET runtime 3.1](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-3.1.22-windows-x64-installer)  

Download and unzip the latest version [here](#).

Navigate to the extracted folder and run ImageBot.exe, this will start the initial setup of the bot.

### Initial setup and authorization.

Once you run the program for the first time, the program will guide you through registering the application with the mastodon server, and authenticating the account that you want to use with the bot.


### Edit the setting in settings.json:
Interval: Time between post in minutes. The default is 60 minutes.  
Visibility: Public = 0, Private = 2, Unlisted = 1, Direct = 3. Use numbers here. The default is 0.  
IsSensitive: Mark post as sensitive. 'true' or 'false'. The default is false.  
