namespace IRBTModUtils {
    public static class ModState {

        public static bool IsDialogueSequencePlaying = false;

        public static void Reset() {
            // Reinitialize state
            IsDialogueSequencePlaying = false;
        }
    }

}
