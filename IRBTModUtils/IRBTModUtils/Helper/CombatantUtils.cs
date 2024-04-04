using BattleTech;
using IRBTModUtils;

namespace us.frostraptor.modUtils {
    public static class CombatantUtils {

        public static string Label(ICombatant combatant) 
        {
            if (combatant == null) { return "UNKNOWN-NULL"; }

            if (SharedState.CombatantLabels != null && SharedState.CombatantLabels.ContainsKey(combatant))
            {
                return SharedState.CombatantLabels[combatant];
            }

            string label = "Unknown";
            if (combatant != null && combatant.GUID != null) 
            {
                string truncatedGUID = combatant.GUID != null ? string.Format("{0:X}", combatant.GUID.GetHashCode()) : "0xDEADBEEF";

                label = $"{combatant.DisplayName}_{truncatedGUID}";
                if (combatant is AbstractActor actor) 
                {
                    try
                    {
                        label = $"{actor.DisplayName}_{actor?.GetPilot()?.Name}_{truncatedGUID}";
                    }
                    catch (System.NotImplementedException)
                    {
                        label = $"CAC_LOSPseudoActor_{truncatedGUID}";
                    }
                } 
            }

            SharedState.CombatantLabels[combatant] = label;

            return label;
        }
    }
}
