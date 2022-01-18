using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ASL.Lib
{
    public class Config
    {
        public Dictionary<string, Mod> Mods { get; set; }
        public List<string> Missions { get; set; }
        public string MissionsPath { get; set; }
        public string ServerPath { get; set; }
        public string ModsPath { get; set; }
        public string ConfigPath { get; set; }
        public string NetworkConfig { get; set; }
        public string BattleEyePath { get; set; }
        public string ExtraArgs { get; set; }

        public string SteamPath { get; set; }
        public string SteamLogin { get; set; }
        public string ServerBranch { get; set; }

        internal void FindMissions()
        {
            if (string.IsNullOrEmpty(MissionsPath))
                throw new Exception("NO MISSIONS PATH");
            Missions = new DirectoryInfo(MissionsPath)
            .GetFiles()
            .Where(x => x.Extension == ".pbo")
            .Select(x => x.Name).ToList();
        }

        internal void FindMods()
        {
            Mods = new Dictionary<string, Mod>();
            if (string.IsNullOrEmpty(ModsPath))
                throw new Exception("NO MODS PATH");
            DirectoryInfo di = new DirectoryInfo(ModsPath);
            var directories = di.GetDirectories();
            var modFolders = directories.Where(x =>
            {
                try
                {
                    return x.GetFiles().Any(x => x.Name == "mod.cpp");
                }
                catch (System.Exception)
                {
                    Console.WriteLine($"ERROR Can't parse {x.FullName}");
                    return false;
                }
            }).ToList();
            foreach (var folder in modFolders)
            {
                var metaData = GetCPPFile(folder.GetFiles().First(x => x.Name == "meta.cpp").FullName);
                Mods.Add(metaData["name"], new Mod
                {
                    Path = folder.FullName,
                    SteamId = metaData["publishedid"]
                });
            }
        }

        private Dictionary<string, string> GetCPPFile(string path)
        {
            var dict = new Dictionary<string, string>();
            var cppData = System.IO.File.ReadAllText(path);
            foreach (var subStr in cppData.Split("\n"))
            {
                if (subStr.Contains("="))
                {
                    var KeyVal = subStr.Split("=");
                    dict.Add(KeyVal[0].Trim(), KeyVal[1].Replace("\"", "").Replace(";", "").Trim());
                }
            }
            return dict;
        }
    }
}