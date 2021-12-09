using BattleTech;
using Harmony;
using IRBTModUtils.Extension;
using IRBTModUtils.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using us.frostraptor.modUtils.CustomDialog;

namespace IRBTModUtils
{
    public static class Mod
    {

        public const string HarmonyPackage = "us.frostraptor.IRBTModUtils";
        public const string LogName = "irbt_mod_utils";

        public static DeferringLogger Log;
        public static string ModDir;
        public static ModConfig Config;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON)
        {
            ModDir = modDirectory;

            Exception configE;
            try
            {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            }
            catch (Exception e)
            {
                configE = e;
                Mod.Config = new ModConfig();
            }
            finally
            {
                Mod.Config.Init();
            }

            Log = new DeferringLogger(modDirectory, LogName, Config.Debug, Config.Trace);

            Log.Debug?.Write($"ModDir is:{modDirectory}");
            Log.Debug?.Write($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info?.Write($"Assembly version: {fvi.ProductVersion}");

            // Try to determine the battletech directory
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            string btDir = Path.GetDirectoryName(fileName);
            Log.Debug?.Write($"BT File is: {fileName} with btDir: {btDir}");
            if (Coordinator.CallSigns == null)
            {
                string filePath = Path.Combine(btDir, Mod.Config.Dialogue.CallsignsPath);
                Mod.Log.Debug?.Write($"Reading files from {filePath}");
                try
                {
                    Coordinator.CallSigns = File.ReadAllLines(filePath).ToList();
                }
                catch (Exception e)
                {
                    Mod.Log.Error?.Write(e, "Failed to read callsigns from BT directory!");
                    Coordinator.CallSigns = new List<string> { "Alpha", "Beta", "Gamma" };
                }
                Mod.Log.Debug?.Write($"Callsign count is: {Coordinator.CallSigns.Count}");
            }

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        // Invoked when ModTek has loaded all mods
        public static void FinishedLoading(List<string> loadOrder, Dictionary<string, Dictionary<string, VersionManifestEntry>> customResources)
        {
        }
    }
}
