using System;
using System.Collections.Generic;
using System.Text;

namespace ImageBot
{
    class Stats
    {
        public int Posts { get; set; }

        public void IncrementPosts()
        {
            Posts++;
        }
    }
}
