using BattleTech;

namespace IRBTModUtils.Extension
{
    public static class CombatantExtensions
    {
        public static string DistinctId(this ICombatant combatant) 
        {
            string label = "Unknown";
            if (combatant != null && combatant.GUID != null)
            {
                string truncatedGUID = combatant.GUID != null ? string.Format("{0:X}", combatant.GUID.GetHashCode()) : "0xDEADBEEF";

                if (combatant is AbstractActor actor)
                {
                    label = $"{actor.DisplayName}_{actor?.GetPilot()?.Name}_{truncatedGUID}";
                }
                else
                {
                    label = $"{combatant.DisplayName}_{truncatedGUID}";
                }

            }
            return label;
        }
    }
}
