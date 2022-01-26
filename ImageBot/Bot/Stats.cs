using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageBot
{
    class Stats
    {
        private static readonly string _defaultFileName = "stats.json";
        private static Stats instance = null;

        private Stats()
        {
            if (!FileHelpers.SerializedFileExists<Stats>(_defaultFileName))
            {
                FileHelpers.CreateNewJsonFile<Stats>(_defaultFileName, this);
            }
            else
            {
                Stats stats = FileHelpers.LoadSerializedFile<Stats>(_defaultFileName);
                this.Posts = stats.Posts;
            }
        }

        public static Stats Instance
        {
            get
            {
                if (instance == null) { instance = new Stats();}
                return instance;
            }
        }

        public int Posts { get; private set; }

        public void IncrementPosts()
        {
            Posts++;
        }

        public void Save()
        {
            FileHelpers.SaveObjectToFile(_defaultFileName, this);
        }
    }
}
