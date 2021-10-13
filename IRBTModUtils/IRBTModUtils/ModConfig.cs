using System.Collections.Generic;
using System.Text;
using us.frostraptor.modUtils;

namespace IRBTModUtils {
    
    public class ModStats {
    }

    public class FeatureState
    {
        public bool EnableMovementModifiers = true;
    }

    public class DialogueConfig
    {
        public string[] Portraits = {
                    "sprites/Portraits/guiTxrPort_DEST_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_01_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_02_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_03_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_04_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_05_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_06_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_07_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_08_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_09_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_10_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_11_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_12_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_davion_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_default_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_kurita_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_liao_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_marik_utr.png",
                    "sprites/Portraits/guiTxrPort_GenericMW_steiner_utr.png",
            };
        public string CallsignsPath = "BattleTech_Data/StreamingAssets/data/nameLists/name_callsign.txt";
    }

    public class SkillModConfig
    {
        public Dictionary<int, int> RatingToModifier = new Dictionary<int, int>();
        public List<string> ModifierBonusAbilities = new List<string>();
        public float BonusMultiplier = 1.0f;
    }

    public class SkillToModsConfig
    {
        public SkillModConfig Gunnery = new SkillModConfig();
        public SkillModConfig Guts = new SkillModConfig();
        public SkillModConfig Piloting = new SkillModConfig();
        public SkillModConfig Tactics = new SkillModConfig();
    }

    public class ModConfig {

        public bool Debug = true;
        public bool Trace = false;

        public FeatureState Features = new FeatureState();

        // This is set to 40m, the minimum required to move one 'hex' no matter the penalties
        public float MinimumMove = 40f;
        public float MinimumJump = 0f;

        public DialogueConfig Dialogue = new DialogueConfig();

        public SkillToModsConfig SkillsToModifiers = new SkillToModsConfig();

        public void LogConfig() {
            Mod.Log.Info?.Write("=== MOD CONFIG BEGIN ===");
            Mod.Log.Info?.Write($"  DEBUG: {this.Debug} Trace: {this.Trace}");
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write("--- FEATURES ---");
            Mod.Log.Info?.Write($"  EnableMovementModifiers: {this.Features.EnableMovementModifiers}");
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write("--- MOVEMENT ---");
            Mod.Log.Info?.Write($"  MinimumMove: {this.MinimumMove}  MinimumJump: {this.MinimumJump}");
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write("--- DIALOGUE OPTIONS ---");
            Mod.Log.Info?.Write($"  CallsignPath: {this.Dialogue.CallsignsPath}");
            Mod.Log.Info?.Write($"  Portraits: {string.Join(",", this.Dialogue.Portraits)}");
            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write("--- SKILL MODIFIERS ---");
            StringBuilder ratToModSB = new StringBuilder();

            foreach (KeyValuePair<int, int> kvp in this.SkillsToModifiers.Gunnery.RatingToModifier)
            {
                ratToModSB.Append($"{kvp.Key}={kvp.Value}");
            }
            Mod.Log.Info?.Write($"Gunnery => ratings: {ratToModSB}");
            Mod.Log.Info?.Write($"  bonus abilities => multiplier: {this.SkillsToModifiers.Gunnery.BonusMultiplier}  " +
                $"abilities: {string.Join(",", this.SkillsToModifiers.Gunnery.ModifierBonusAbilities)}");

            foreach (KeyValuePair<int, int> kvp in this.SkillsToModifiers.Guts.RatingToModifier)
            {
                ratToModSB.Append($"{kvp.Key}={kvp.Value}");
            }
            Mod.Log.Info?.Write($"Guts => ratings: {ratToModSB}");
            Mod.Log.Info?.Write($"  bonus abilities => multiplier: {this.SkillsToModifiers.Guts.BonusMultiplier}  " +
                $"abilities: {string.Join(",", this.SkillsToModifiers.Guts.ModifierBonusAbilities)}");

            foreach (KeyValuePair<int, int> kvp in this.SkillsToModifiers.Piloting.RatingToModifier)
            {
                ratToModSB.Append($"{kvp.Key}={kvp.Value}");
            }
            Mod.Log.Info?.Write($"Piloting => ratings: {ratToModSB}");
            Mod.Log.Info?.Write($"  bonus abilities => multiplier: {this.SkillsToModifiers.Piloting.BonusMultiplier}  " +
                $"abilities: {string.Join(",", this.SkillsToModifiers.Piloting.ModifierBonusAbilities)}");

            foreach (KeyValuePair<int, int> kvp in this.SkillsToModifiers.Tactics.RatingToModifier)
            {
                ratToModSB.Append($"{kvp.Key}={kvp.Value}");
            }
            Mod.Log.Info?.Write($"Tactics => ratings: {ratToModSB}");
            Mod.Log.Info?.Write($"  bonus abilities => multiplier: {this.SkillsToModifiers.Tactics.BonusMultiplier}  " +
                $"abilities: {string.Join(",", this.SkillsToModifiers.Tactics.ModifierBonusAbilities)}");

            Mod.Log.Info?.Write("");

            Mod.Log.Info?.Write("=== MOD CONFIG END ===");
        }

        public void Init()
        {
            // Init skillsToMods
            if (this.SkillsToModifiers.Gunnery.RatingToModifier.Count == 0)
                this.SkillsToModifiers.Gunnery.RatingToModifier = new Dictionary<int, int>(SkillUtils.LegacyModsBySkill);
            if (this.SkillsToModifiers.Guts.RatingToModifier.Count == 0)
                this.SkillsToModifiers.Guts.RatingToModifier = new Dictionary<int, int>(SkillUtils.LegacyModsBySkill);
            if (this.SkillsToModifiers.Piloting.RatingToModifier.Count == 0)
                this.SkillsToModifiers.Piloting.RatingToModifier = new Dictionary<int, int>(SkillUtils.LegacyModsBySkill);
            if (this.SkillsToModifiers.Tactics.RatingToModifier.Count == 0)
                this.SkillsToModifiers.Tactics.RatingToModifier = new Dictionary<int, int>(SkillUtils.LegacyModsBySkill);

            if (this.SkillsToModifiers.Gunnery.ModifierBonusAbilities.Count == 0)
                this.SkillsToModifiers.Gunnery.ModifierBonusAbilities = new List<string>() { "AbilityDefG5", "AbilityDefG8" };
            if (this.SkillsToModifiers.Guts.ModifierBonusAbilities.Count == 0)
                this.SkillsToModifiers.Guts.ModifierBonusAbilities = new List<string>() { "AbilityDefGu5", "AbilityDefGu8" };
            if (this.SkillsToModifiers.Piloting.ModifierBonusAbilities.Count == 0)
                this.SkillsToModifiers.Piloting.ModifierBonusAbilities = new List<string>() { "AbilityDefP5", "AbilityDefP8" };
            if (this.SkillsToModifiers.Tactics.ModifierBonusAbilities.Count == 0)
                this.SkillsToModifiers.Tactics.ModifierBonusAbilities = new List<string>() { "AbilityDefT5A", "AbilityDefT8A" };
        }
    }
}
