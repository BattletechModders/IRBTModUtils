using BattleTech;
using BattleTech.UI;
using System.Collections;

namespace IRBTModUtils {
    // State that only *this* mod cares about or has access to
    public static class ModState {

        public static Queue DialogueQueue = new Queue();
        public static bool IsDialogStackActive = false;

        public static void Reset() {
            // Reinitialize state
            DialogueQueue.Clear();
            IsDialogStackActive = false;
        }
    }

}
