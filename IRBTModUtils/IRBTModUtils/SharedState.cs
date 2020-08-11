using BattleTech;
using BattleTech.UI;
using System.Collections.Generic;

namespace IRBTModUtils {
    // Common, global state references that many mods may find helpful
    public static class SharedState {

        // Common shared state
        public static CombatGameState Combat = null;
        public static CombatHUD CombatHUD = null;
        public static Dictionary<ICombatant, string> CombatantLabels = new Dictionary<ICombatant, string>();

        public static void Reset() {
            Combat = null;
            CombatHUD = null;
            CombatantLabels.Clear();
        }
    }

}
