using IRBTModUtils.Extension;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IRBTModUtils {
    // State that only *this* mod cares about or has access to
    public static class ModState {

        public static Queue DialogueQueue = new Queue();
        public static bool IsDialogStackActive = false;
        public static Dictionary<string, Sprite> Portraits = new Dictionary<string, Sprite>();
        public static List<MechMoveModifier> MoveModifiers = new List<MechMoveModifier>();

        public static void Reset() {
            // Reinitialize state
            DialogueQueue.Clear();
            IsDialogStackActive = false;
            MoveModifiers.Clear();
        }
    }

}
