using BattleTech;
using BattleTech.UI;
using Harmony;
using IRBTModUtils;
using System;
using us.frostraptor.modUtils.CustomDialog;

namespace us.frostraptor.modUtils {

    // Initialize shared elements (CombatGameState, etc)
    [HarmonyPatch(typeof(CombatGameState), "_Init")]
    [HarmonyPatch(new Type[] { typeof(GameInstance), typeof(Contract), typeof(string) })]
    public static class CombatGameState__Init
    {
        public static void Postfix(CombatGameState __instance) 
        {
            SharedState.Combat = __instance;
        }
    }

    // Teardown shared elements to prevent NREs
    [HarmonyPatch(typeof(CombatGameState), "OnCombatGameDestroyed")]
    public static class CombatGameState_OnCombatGameDestroyed
    {
        public static void Prefix() 
        {
            SharedState.Combat = null;
        }
    }
}
