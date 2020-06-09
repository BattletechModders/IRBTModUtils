using BattleTech;
using BattleTech.UI;

namespace IRBTModUtils {
    // Common, global state references that many mods may find helpful
    public static class SharedState {

        // Common shared state
        public static CombatGameState Combat = null;
        public static CombatHUD CombatHUD = null;

        public static void Reset() {
            Combat = null;
            CombatHUD = null;
        }
    }

}
