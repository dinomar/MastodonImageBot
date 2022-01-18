using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBot.Bot
{
    class Settings
    {
        public int Interval { get; set; } = 60; // minutes
        public string Folder1 { get; set; } = "images1";
        public string Folder2 { get; set; } = "images2";
        public string CurrentFolder { get; set; } = "images1";
    }
}
