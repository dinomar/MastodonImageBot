using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageBot
{
    static class StatsManager
    {
        private static readonly string _defaultFileName = "stats.json";

        public static Stats GetStats()
        {
            if (!FileHelpers.CheckSerializedFileExists<Stats>(_defaultFileName))
            {
                Console.WriteLine("Create new file");
                return FileHelpers.CreateNewJsonFile<Stats>(_defaultFileName, new Stats());
            }
            else
            {
                return FileHelpers.LoadSerializedFile<Stats>(_defaultFileName);
            }
        }

        public static void CreateNewStatsFile()
        {
            FileHelpers.CreateFileIfNotExists<Stats>(_defaultFileName, new Stats());
        }

        public static void SaveStats(Stats stats)
        {
            FileHelpers.SaveObjectToFile(_defaultFileName, stats);
        }
    }
}
