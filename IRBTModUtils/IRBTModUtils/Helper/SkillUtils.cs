using BattleTech;
using IRBTModUtils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace us.frostraptor.modUtils {

    public class SkillUtils {

        // A mapping of skill level to modifier
        public static readonly Dictionary<int, int> LegacyModsBySkill = new Dictionary<int, int> {
            { 1, 0 },
            { 2, 1 },
            { 3, 1 },
            { 4, 2 },
            { 5, 2 },
            { 6, 3 },
            { 7, 3 },
            { 8, 4 },
            { 9, 4 },
            { 10, 5 },
            { 11, 6 },
            { 12, 7 },
            { 13, 8 }
        };

        private const string TOOLTIP_GREEN = "#00FF00";
        private const string TOOLTIP_RED = "#FF0000";

        public static int GetGunneryModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Gunnery, Mod.Config.SkillsToModifiers.Gunnery);
        }

        public static int GetGutsModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Guts, Mod.Config.SkillsToModifiers.Guts);
        }

        public static int GetPilotingModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Piloting, Mod.Config.SkillsToModifiers.Piloting);
        }

        public static int GetTacticsModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Tactics, Mod.Config.SkillsToModifiers.Tactics);
        }

        // Process any tags that provide flat bonuses
        public static int GetTagsModifier(Pilot pilot, Dictionary<string,int> tagsToModifiers) {
            int mod = 0;

            Dictionary<string, int> foundTags = new Dictionary<string, int>();
            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (tagsToModifiers.ContainsKey(tag)) {
                    foundTags[tag] = tagsToModifiers[tag];
                }
            }

            foreach (int modifier in foundTags.Values) {
                mod += modifier;
            }

            return mod;
        }

        // Generates tooltip details for tags that provide modifiers
        public static List<string> ModifierTagsToolips(Pilot pilot, Dictionary<string, int> tagsToModifiers, 
            int space = 2, bool invert=false) {

            Dictionary<string, int> foundTags = new Dictionary<string, int>();
            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (tagsToModifiers.ContainsKey(tag)) {
                    foundTags[tag] = tagsToModifiers[tag];
                }
            }

            List<string> details = new List<string>();
            foreach (KeyValuePair<string, int> kvp in foundTags) {
                if (kvp.Value > 0) {
                    string color = invert ? TOOLTIP_GREEN : TOOLTIP_RED;
                    details.Add($"<space={space}em><color={color}>{kvp.Key}: {kvp.Value:+0}</color>");
                } else if (kvp.Value < 0) {
                    string color = invert ? TOOLTIP_RED : TOOLTIP_GREEN;
                    details.Add($"<space={space}em><color={color}>{kvp.Key}: {kvp.Value}</color>");
                }
            }

            return details;
        }

        // Generates tooltip details for tags that have a non-modifier effect.
        public static List<string> SpecialTagsTooltips(Pilot pilot, Dictionary<string, int> tagsToSpecials, int space = 2) {

            Dictionary<string, int> foundTags = new Dictionary<string, int>();
            foreach (string tag in pilot.pilotDef.PilotTags) {
                if (tagsToSpecials.ContainsKey(tag)) {
                    foundTags[tag] = tagsToSpecials[tag];
                }
            }

            List<string> details = new List<string>();
            foreach (KeyValuePair<string, int> kvp in foundTags) {
                details.Add($"<space={space}em>{kvp.Key}: <i>{kvp.Value}</i>");
            }

            return details;
        }

        // --- PRIVATE BELOW ---

        [Obsolete("Please use GetModifier instead")]
        public static int NormalizeSkill(int rawValue) {
            int normalizedVal = rawValue;
            if (rawValue >= 11 && rawValue <= 14) {
                // 11, 12, 13, 14 normalizes to 11
                normalizedVal = 11;
            } else if (rawValue >= 15 && rawValue <= 18) {
                // 15, 16, 17, 18 normalizes to 14
                normalizedVal = 12;
            } else if (rawValue == 19 || rawValue == 20) {
                // 19, 20 normalizes to 13
                normalizedVal = 13;
            } else if (rawValue <= 0) {
                normalizedVal = 1;
            } else if (rawValue > 20) {
                normalizedVal = 13;
            }
            return normalizedVal;
        }

        // Legacy implementation, assumes the modifier spread in RT
        private static int GetModifier(Pilot pilot, int skillValue, string abilityDefIdL5, string abilityDefIdL8) {
            return GetModifier(pilot, skillValue, LegacyModsBySkill, new List<string>() { abilityDefIdL5, abilityDefIdL8 }, 1);
        }

        public static int GetModifier(Pilot pilot, int skillValue, SkillModConfig skillModConfig)
        {
            if (pilot == null) return 0;
            Mod.Log.Debug?.Write($"Calculating modifier for pilot: {pilot.Name}_{pilot.GUID}");

            skillModConfig.RatingToModifier.TryGetValue(skillValue, out int modifier);
            Mod.Log.Debug?.Write($"  modifier: {modifier} from skillValue: {skillValue}");

            int abilityMod = 0;
            if (skillModConfig.ModifierBonusAbilities != null && skillModConfig.ModifierBonusAbilities.Count > 0)
            {
                int matchedAbilities = MatchedAbilities(pilot, skillModConfig.ModifierBonusAbilities);
                abilityMod = (int)Math.Ceiling(matchedAbilities * skillModConfig.BonusMultiplier);
                Mod.Log.Debug?.Write($"  matchedAbilityCount: {matchedAbilities} x abilityMulti: {skillModConfig.BonusMultiplier} = {abilityMod}");
            }

            return modifier + abilityMod;
        }

        // Customizable modifier spread as per BD's request
        public static int GetModifier(Pilot pilot, int skillValue, Dictionary<int, int> modifiersForSkill, 
            List<string> abilityDefIds, float abilityMulti = 1f)
        {
            if (pilot == null) return 0;
            Mod.Log.Debug?.Write($"Calculating modifier for pilot: {pilot.Name}_{pilot.GUID}");

            modifiersForSkill.TryGetValue(skillValue, out int modifier);
            Mod.Log.Debug?.Write($"  modifier: {modifier} from skillValue: {skillValue}");

            int abilityMod = 0;
            if (abilityDefIds != null && abilityDefIds.Count > 0)
            {
                int matchedAbilities = MatchedAbilities(pilot, abilityDefIds);
                abilityMod = (int)Math.Ceiling(matchedAbilities * abilityMulti);
                Mod.Log.Debug?.Write($"  matchedAbilityCount: {matchedAbilities} x abilityMulti: {abilityMulti} = {abilityMod}");
            }

            return modifier + abilityMod;
        }

        private static int MatchedAbilities(Pilot pilot, List<string> abilityDefIds)
        {
            if (pilot == null || pilot.Abilities == null || pilot.Abilities.Count == 0 ||
                abilityDefIds == null || abilityDefIds.Count == 0) return 0;

            List<string> lcAbilityDefIds = abilityDefIds.Select(aDefId => aDefId.ToLower()).ToList();

            int matchedCount = 0;
            matchedCount = pilot.Abilities.Where(a => a != null && a.Def != null && a.Def.Id != null)
                .Select(a => a.Def.Id.ToLower())
                .Where(aDefId => lcAbilityDefIds.Contains(aDefId))
                .Count();

            return matchedCount;
        }
    }
}
