using Disboard.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBot.Configuration
{
    class Config
    {
        public bool IsConfigured { get; set; }
        public string Instance { get; set; }
        public string ApplicationName { get; set; }
        public Credential Credential { get; set; }
    }
}
