using System.Collections;

namespace IRBTModUtils {
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
