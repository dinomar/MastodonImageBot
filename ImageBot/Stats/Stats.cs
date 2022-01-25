using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBot.Stats
{
    class Stats
    {
        public int Posts { get; private set; }

        public void IncrementPosts()
        {
            Posts++;
        }
    }
}
