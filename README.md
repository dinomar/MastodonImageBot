# MastodonImageBot

## Instructions

#### Windows:
Download and install [Visual C++ Redistributable for Visual Studio 2015](https://www.microsoft.com/en-in/download/details.aspx?id=48145)
Download and install [.NET Core 3.1 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-3.1.22-windows-x64-installer)  

Download and unzip the latest version [here](https://github.com/dinomar/MastodonImageBot/releases/tag/V1.0.0).

Navigate to the extracted folder and run ImageBot.exe, this will start the initial setup of the bot.

### Initial setup and authorization.

When you run the program for the first time, the program will guide you through registering the application with the mastodon server, and authenticating the account that you want to use with the bot.

When the setup is complete, exit the program and copy the images that you want to post into the 'images1' folder that was created during the setup. You may also want to change the settings in the settings.json file.

### Edit the setting in settings.json:
Interval: Time between post in minutes. The default is 60 minutes.  
Visibility: Public = 0, Private = 2, Unlisted = 1, Direct = 3. Use numbers here. The default is 0.  
IsSensitive: Mark post as sensitive. 'true' or 'false'. The default is false.  

### How the bot works
This bot will pick a random image from the 'images1' folder.  
Post that image, then move it to the 'images2' folder.  
When the 'images1' folder no longer has any images left inside it. The folders swap around, and images will be selected from the 'images2' folder and move to the 'images1' folder once they have been posted.  
The bot continues to switch the source and deposit folders around indefinitely.  
