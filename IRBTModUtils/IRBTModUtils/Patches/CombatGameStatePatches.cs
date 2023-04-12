using IRBTModUtils;
using System;
using System.IO;
using UnityEngine;

namespace us.frostraptor.modUtils {

    // Initialize shared elements (CombatGameState, etc)
    [HarmonyPatch(typeof(CombatGameState), "_Init")]
    [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(Contract), typeof(string) })]
    public static class CombatGameState__Init
    {
        [HarmonyPostfix]
        public static void Postfix(CombatGameState __instance) 
        {
            SharedState.Combat = __instance;

            // Load any dialogue portraits at startup
            if (ModState.Portraits.Count == 0)
            {
                Mod.Log.Info?.Write($"Loading {Mod.Config.Dialogue.Portraits} portrait sprites.");
                foreach (string portraitPath in Mod.Config.Dialogue.Portraits)
                {
                    string path = Utilities.PathUtils.AppendPath(EmotePortrait.SpriteBasePath, portraitPath, appendForwardSlash: false);
                    if (File.Exists(path))
                    {
                        Sprite sprite = Utilities.ImageUtils.LoadSprite(path);
                        Mod.Log.Info?.Write($"Added sprite for portraitPath: {portraitPath}");
                        ModState.Portraits.Add(portraitPath, sprite);
                    }
                    else
                    {
                        Mod.Log.Warn?.Write("$Failed to load portrait at path: {path}!");
                    }
                }
                Mod.Log.Info?.Write($"Loaded {ModState.Portraits.Keys.Count} portraits.");
            }
        }
    }

    // Teardown shared elements to prevent NREs
    [HarmonyPatch(typeof(CombatGameState), "OnCombatGameDestroyed")]
    public static class CombatGameState_OnCombatGameDestroyed
    {
        [HarmonyPrefix]
        public static void Prefix(ref bool __runOriginal) 
        {
            SharedState.ResetOnCombatEnd();
        }
    }
}
