using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBot.Configuration
{
    class LogEventArgs
    {
        public string Message { get; private set; }

        public LogEventArgs(string message)
        {
            Message = message;
        }
    }
}
