using Harmony;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using us.frostraptor.modUtils.CustomDialog;
using us.frostraptor.modUtils.logging;

namespace IRBTModUtils {
    public static class Mod {

        public const string HarmonyPackage = "us.frostraptor.IRBTModUtils";
        public const string LogName = "irbt_mod_utils";

        public static IntraModLogger Log;
        public static string ModDir;
        public static ModConfig Config;

        public static readonly Random Random = new Random();

        public static void Init(string modDirectory, string settingsJSON) {
            ModDir = modDirectory; 

            Exception settingsE = null;
            try {
                Mod.Config = JsonConvert.DeserializeObject<ModConfig>(settingsJSON);
            } catch (Exception e) {
                settingsE = e;
                Mod.Config = new ModConfig();
            }

            Log = new IntraModLogger(modDirectory, LogName, Config.Debug, Config.Trace);

            Assembly asm = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(asm.Location);
            Log.Info($"Assembly version: {fvi.ProductVersion}");

            Log.Debug($"ModDir is:{modDirectory}");
            Log.Debug($"mod.json settings are:({settingsJSON})");
            Mod.Config.LogConfig();

            if (settingsE != null) {
                Log.Info($"ERROR reading settings file! Error was: {settingsE}");
            } else {
                Log.Info($"INFO: No errors reading settings file.");
            }

            // Try to determine the battletech directory
            string fileName = Process.GetCurrentProcess().MainModule.FileName;
            string btDir = Path.GetDirectoryName(fileName);
            Log.Debug($"BT File is: {fileName} with btDir: {btDir}");
            if (Coordinator.CallSigns == null) {
                string filePath = Path.Combine(btDir, Mod.Config.Dialogue.CallsignsPath);
                Mod.Log.Debug($"Reading files from {filePath}");
                try {
                    Coordinator.CallSigns = File.ReadAllLines(filePath).ToList();
                } catch (Exception e) {
                    Mod.Log.Error("Failed to read callsigns from BT directory!");
                    Mod.Log.Error(e);
                    Coordinator.CallSigns = new List<string> { "Alpha", "Beta", "Gamma" };
                }
                Mod.Log.Debug($"Callsign count is: {Coordinator.CallSigns.Count}");
            }

            var harmony = HarmonyInstance.Create(HarmonyPackage);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }
}
