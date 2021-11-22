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
using us.frostraptor.modUtils.logging;

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
        public static void FinishedLoading()
        {
            Mod.Log.Info?.Write($"Checking for UnitMoveModifierTypes in assemblies");
            // Record custom types from mods
            foreach (var type in GetAllTypesThatImplementInterface<MechMoveModifier>())
            {
                Mod.Log.Info?.Write($"Adding move modifier: {type.FullName}");
                MechMoveModifier instance = (MechMoveModifier)Activator.CreateInstance(type);
                ModState.MoveModifiers.Add(instance);
            }
        }
        private static bool CheckBlockList(Assembly assembly)
        {
            foreach (string name in Mod.Config.BlockedDlls) { if (assembly.FullName.StartsWith(name)) { return true; } }
            return false;
        }

        private static IEnumerable<Type> GetAllTypesThatImplementInterface<T>()
        {
            var targetType = typeof(T);
            List<Type> result = new List<Type>();
            try
            {
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (CheckBlockList(assembly)) { continue; }
                    try
                    {
                        Type[] types = assembly.GetTypes();
                        foreach (Type type in types)
                        {
                            try
                            {
                                if (type.IsInterface) { continue; }
                                if (type.IsAbstract) { continue; }
                                if (targetType.IsAssignableFrom(type) == false) { continue; }
                                result.Add(type);
                            }
                            catch (Exception e)
                            {
                                Mod.Log.Error?.Write(assembly.FullName);
                                Mod.Log.Error?.Write(type.FullName);
                                Mod.Log.Error?.Write(e.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Mod.Log.Error?.Write(assembly.FullName);
                        Mod.Log.Error?.Write(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Mod.Log.Error?.Write(e.ToString());
            }
            return result;
            //return AppDomain.CurrentDomain.GetAssemblies()
            //    .Where(a => !a.IsDynamic)
            //    .SelectMany(s => s.GetTypes())
            //    .Where(p => !p.IsInterface && !p.IsAbstract)
            //    .Where(p => targetType.IsAssignableFrom(p));
        }
    }
}
