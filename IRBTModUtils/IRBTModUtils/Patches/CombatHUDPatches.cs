using BattleTech;
using BattleTech.UI;
using Harmony;
using IRBTModUtils;
using System;
using us.frostraptor.modUtils.CustomDialog;

namespace us.frostraptor.modUtils {

   // CombatDialog Patches
   // Register listeners for our events, using the CombatHUD hook
   [HarmonyPatch(typeof(CombatHUD), "SubscribeToMessages")]
    public static class CombatHUD_SubscribeToMessages {
        public static void Postfix(CombatHUD __instance, bool shouldAdd) {
            if (__instance != null) {
                __instance.Combat.MessageCenter.Subscribe(
                    (MessageCenterMessageType)MessageTypes.OnCustomDialog, new ReceiveMessageCenterMessage(Coordinator.OnCustomDialogMessage), shouldAdd);
            }
        }
    }

    // Initialize shared elements (CombatGameState, etc)
    [HarmonyPatch(typeof(CombatHUD), "Init")]
    [HarmonyPatch(new Type[] { typeof(CombatGameState) })]
    public static class CombatHUD_Init {
        public static void Postfix(CombatHUD __instance, CombatGameState Combat) {
            SharedState.CombatHUD = __instance;

            Coordinator.OnCombatHUDInit(Combat, __instance);
        }
    }

    // Teardown shared elements to prevent NREs
    [HarmonyPatch(typeof(CombatHUD), "OnCombatGameDestroyed")]
    public static class CombatHUD_OnCombatGameDestroyed {
        public static void Prefix() {
            Coordinator.OnCombatGameDestroyed();
            
            SharedState.CombatHUD = null;
        }
    }
}
