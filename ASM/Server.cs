using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ASM.Lib
{
    public class Server
    {
        public string MissionsPath { get; set; }
        public string ServerPath { get; set; }
        public string ModsPath { get; set; }
        public string ConfigPath { get; set; }
        public string NetworkConfig { get; set; }
        public string BattleEyePath { get; set; }
        public string ExtraArgs { get; set; }
        public string ServerBranch { get; set; }
        [JsonIgnore]
        public Dictionary<string, Mod> Mods { get; set; } = new Dictionary<string, Mod>();
        [JsonIgnore]
        public List<string> Missions { get; set; } = new List<string>();
        [JsonIgnore]
        public Dictionary<string, Template> Templates { get; set; } = new Dictionary<string, Template>();

        private void FindMissions()
        {
            if (string.IsNullOrEmpty(MissionsPath))
                throw new Exception("NO MISSIONS PATH");
            Missions = new DirectoryInfo(MissionsPath)
            .GetFiles()
            .Where(x => x.Extension == ".pbo")
            .Select(x => x.Name).ToList();
        }

        private void FindMods()
        {
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
                if (Mods.ContainsKey(metaData["name"]))
                    continue;
                Mods.Add(metaData["name"], new Mod
                {
                    Path = folder.FullName,
                    SteamId = metaData["publishedid"]
                });
            }
        }

        internal void Load()
        {
            FindMissions();
            FindMods();
        }

        internal GeneratedServer GetGeneratedServer() =>
            new GeneratedServer
            {
                Mods = Mods,
                Missions = Missions,
                Templates = Templates,
            };

        public void ToggleServerSide(List<string> modIds)
        {
            var mods = Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();
            foreach (var mod in mods)
            {
                mod.ServerSide = !mod.ServerSide;
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