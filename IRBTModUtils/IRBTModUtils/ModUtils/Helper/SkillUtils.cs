using BattleTech;
using System.Collections.Generic;

namespace us.frostraptor.modUtils {

    public class SkillUtils {

        private const string TOOLTIP_GREEN = "#00FF00";
        private const string TOOLTIP_RED = "#FF0000";

        public static int GetGunneryModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Gunnery, "AbilityDefG5", "AbilityDefG8");
        }

        public static int GetGutsModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Guts, "AbilityDefGu5", "AbilityDefGu8");
        }

        public static int GetPilotingModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Piloting, "AbilityDefP5", "AbilityDefP8");
        }

        public static int GetTacticsModifier(Pilot pilot) {
            return GetModifier(pilot, pilot.Tactics, "AbilityDefT5A", "AbilityDefT8A");
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

        // A mapping of skill level to modifier
        private static readonly Dictionary<int, int> ModifierBySkill = new Dictionary<int, int> {
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

        private static int GetModifier(Pilot pilot, int skillValue, string abilityDefIdL5, string abilityDefIdL8) {
            int normalizedVal = NormalizeSkill(skillValue);
            int modifier = ModifierBySkill[normalizedVal];

            string aDefL5 = abilityDefIdL5?.ToLower(), aDefL8 = abilityDefIdL8?.ToLower();
            bool hasL5 = false, hasL8 = false;
            foreach (Ability ability in pilot.Abilities) {
                string abilityId = ability?.Def?.Id?.ToLower();
                hasL5 |= abilityId != null && abilityId.Equals(aDefL5);
                hasL8 |= abilityId != null && abilityId.Equals(aDefL8);
            }
            if (hasL5) { modifier++; }
            if (hasL8) { modifier++; }

            return modifier;
        }
    }
}
