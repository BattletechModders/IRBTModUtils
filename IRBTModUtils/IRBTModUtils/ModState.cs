using IRBTModUtils.CustomInfluenceMap;
using IRBTModUtils.Feature;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IRBTModUtils {
    // State that only *this* mod cares about or has access to
    public static class ModState {

        public static Queue DialogueQueue = new Queue();
        public static bool IsDialogStackActive = false;
        public static Dictionary<string, Sprite> Portraits = new Dictionary<string, Sprite>();

        internal static List<MechMoveDistanceModifier> ExtMovementMods = new List<MechMoveDistanceModifier>();

        internal static List<CustomInfluenceMapAllyFactor> CustomAllyFactors = new List<CustomInfluenceMapAllyFactor>();
        internal static List<CustomInfluenceMapHostileFactor> CustomHostileFactors = new List<CustomInfluenceMapHostileFactor>();
        internal static List<CustomInfluenceMapPositionFactor> CustomPositionFactors = new List<CustomInfluenceMapPositionFactor>();

        internal static List<CustomInfluenceMapAllyFactor> RemovedAllyFactors = new List<CustomInfluenceMapAllyFactor>();
        internal static List<CustomInfluenceMapHostileFactor> RemovedHostileFactors = new List<CustomInfluenceMapHostileFactor>();
        internal static List<CustomInfluenceMapPositionFactor> RemovedPositionFactors = new List<CustomInfluenceMapPositionFactor>();

        public static void Reset() {
            // Reinitialize state
            DialogueQueue.Clear();
            IsDialogStackActive = false;             
        }
    }

}
