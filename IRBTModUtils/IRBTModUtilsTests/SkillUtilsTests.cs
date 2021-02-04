using BattleTech;
using Harmony;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using us.frostraptor.modUtils;

namespace IRBTModUtilsTests
{
    [TestClass]
    public class SkillUtilsTests
    {

        [TestMethod] 
        public void LegacyGetModifier_Base()
        {
            FactionValue factionValue = new FactionValue();
            factionValue.Name = "1";

            HumanDescriptionDef humanDescDef = new HumanDescriptionDef("-1", "Test", "FNAME", "LNAME", "CSIGN", Gender.Male, factionValue, 1, "foo", "");
            Traverse.Create(humanDescDef).Field("factionValue").SetValue(factionValue);
            Traverse.Create(humanDescDef).Field("factionID").SetValue("1");

            // gun, pilot, guts, tactics
            PilotDef pilotDefHigh = new PilotDef(humanDescDef, 10, 9, 8, 7, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotHigh = new Pilot(pilotDefHigh, "-1", false);

            Assert.AreEqual(5, SkillUtils.GetGunneryModifier(pilotHigh));
            Assert.AreEqual(4, SkillUtils.GetPilotingModifier(pilotHigh));
            Assert.AreEqual(4, SkillUtils.GetGutsModifier(pilotHigh));
            Assert.AreEqual(3, SkillUtils.GetTacticsModifier(pilotHigh));

            PilotDef pilotDefMed = new PilotDef(humanDescDef, 7, 6, 5, 4, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotMed = new Pilot(pilotDefMed, "-1", false);

            Assert.AreEqual(3, SkillUtils.GetGunneryModifier(pilotMed));
            Assert.AreEqual(3, SkillUtils.GetPilotingModifier(pilotMed));
            Assert.AreEqual(2, SkillUtils.GetGutsModifier(pilotMed));
            Assert.AreEqual(2, SkillUtils.GetTacticsModifier(pilotMed));

            PilotDef pilotDefLog = new PilotDef(humanDescDef, 4, 3, 2, 1, 0, 3, false, 0, "voice", 
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotLow = new Pilot(pilotDefLog, "-1", false);

            Assert.AreEqual(2, SkillUtils.GetGunneryModifier(pilotLow));
            Assert.AreEqual(1, SkillUtils.GetPilotingModifier(pilotLow));
            Assert.AreEqual(1, SkillUtils.GetGutsModifier(pilotLow));
            Assert.AreEqual(0, SkillUtils.GetTacticsModifier(pilotLow));
        }

        [TestMethod]
        public void LegacyGetModifier_Abilities()
        {
            AbilityDef gutsDef8 = new AbilityDef();
            Traverse.Create(gutsDef8).Property("Description").SetValue(new BaseDescriptionDef("AbilityDefG8", "ABC", "DEF", "-1"));
            AbilityDef gutsDef5 = new AbilityDef();
            Traverse.Create(gutsDef5).Property("Description").SetValue(new BaseDescriptionDef("AbilityDefG5", "ABC", "DEF", "-1"));

            FactionValue factionValue = new FactionValue();
            factionValue.Name = "1";

            HumanDescriptionDef humanDescDef = new HumanDescriptionDef("-1", "Test", "FNAME", "LNAME", "CSIGN", Gender.Male, factionValue, 1, "foo", "");
            Traverse.Create(humanDescDef).Field("factionValue").SetValue(factionValue);
            Traverse.Create(humanDescDef).Field("factionID").SetValue("1");

            // gun, pilot, guts, tactics
            PilotDef pilotDefHigh = new PilotDef(humanDescDef, 10, 9, 8, 7, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotHigh = new Pilot(pilotDefHigh, "-1", false);
            Traverse.Create(pilotHigh).Property("Abilities").SetValue(new List<Ability>());

            pilotHigh.Abilities.Add(new Ability(gutsDef8));
            pilotHigh.Abilities.Add(new Ability(gutsDef5));

            Assert.AreEqual(7, SkillUtils.GetGunneryModifier(pilotHigh));
            Assert.AreEqual(4, SkillUtils.GetPilotingModifier(pilotHigh));
            Assert.AreEqual(4, SkillUtils.GetGutsModifier(pilotHigh));
            Assert.AreEqual(3, SkillUtils.GetTacticsModifier(pilotHigh));

            PilotDef pilotDefMed = new PilotDef(humanDescDef, 7, 6, 5, 4, 0, 3, false, 0, "voice",
                new List<string>() { "AbilityDefG5" }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotMed = new Pilot(pilotDefMed, "-1", false);
            Traverse.Create(pilotMed).Property("Abilities").SetValue(new List<Ability>());

            pilotMed.Abilities.Add(new Ability(gutsDef5));

            Assert.AreEqual(4, SkillUtils.GetGunneryModifier(pilotMed));
            Assert.AreEqual(3, SkillUtils.GetPilotingModifier(pilotMed));
            Assert.AreEqual(2, SkillUtils.GetGutsModifier(pilotMed));
            Assert.AreEqual(2, SkillUtils.GetTacticsModifier(pilotMed));

            PilotDef pilotDefLog = new PilotDef(humanDescDef, 4, 3, 2, 1, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotLow = new Pilot(pilotDefLog, "-1", false);

            Assert.AreEqual(2, SkillUtils.GetGunneryModifier(pilotLow));
            Assert.AreEqual(1, SkillUtils.GetPilotingModifier(pilotLow));
            Assert.AreEqual(1, SkillUtils.GetGutsModifier(pilotLow));
            Assert.AreEqual(0, SkillUtils.GetTacticsModifier(pilotLow));
        }

        [TestMethod]
        public void GetModifier_Abilities()
        {
            Dictionary<int, int> modifiersForSkill = new Dictionary<int, int>()
            {
                { 1, 5 }, { 2, 10 }, { 3, 15 }, { 4, 20 }, { 5, 25 }, { 6, 30 }, { 7, 35 }, { 8, 40 }, { 9, 45 }, { 10, 50 },
                { 11, 55 }, { 12, 60 }, { 13, 65 }, { 14, 70 }, { 15, 75 }, { 16, 80 }, { 17, 85 }, { 18, 90 }, { 19, 95 }, { 20, 100 },
            };
            float abilityMulti = 1.5f;

            AbilityDef gutsDef8 = new AbilityDef();
            Traverse.Create(gutsDef8).Property("Description").SetValue(new BaseDescriptionDef("AbilityDefG8", "ABC", "DEF", "-1"));
            AbilityDef gutsDef5 = new AbilityDef();
            Traverse.Create(gutsDef5).Property("Description").SetValue(new BaseDescriptionDef("AbilityDefG5", "ABC", "DEF", "-1"));

            FactionValue factionValue = new FactionValue();
            factionValue.Name = "1";

            HumanDescriptionDef humanDescDef = new HumanDescriptionDef("-1", "Test", "FNAME", "LNAME", "CSIGN", Gender.Male, factionValue, 1, "foo", "");
            Traverse.Create(humanDescDef).Field("factionValue").SetValue(factionValue);
            Traverse.Create(humanDescDef).Field("factionID").SetValue("1");

            // gun, pilot, guts, tactics
            PilotDef pilotDefHigh = new PilotDef(humanDescDef, 10, 9, 8, 7, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotHigh = new Pilot(pilotDefHigh, "-1", false);
            Traverse.Create(pilotHigh).Property("Abilities").SetValue(new List<Ability>());

            pilotHigh.Abilities.Add(new Ability(gutsDef8));
            pilotHigh.Abilities.Add(new Ability(gutsDef5));

            Assert.AreEqual(53, 
                SkillUtils.GetModifier(pilotHigh, pilotHigh.Gunnery, modifiersForSkill, new List<string>() { "AbilityDefG5", "AbilityDefG8" }, abilityMulti)
                );
            Assert.AreEqual(45,
                SkillUtils.GetModifier(pilotHigh, pilotHigh.Piloting, modifiersForSkill, new List<string>() { "AbilityDefP5", "AbilityDefP8" }, abilityMulti)
                );
            Assert.AreEqual(40,
                SkillUtils.GetModifier(pilotHigh, pilotHigh.Guts, modifiersForSkill, new List<string>() { "AbilityDefGu5", "AbilityDefGu8" }, abilityMulti)
                );
            Assert.AreEqual(35,
                SkillUtils.GetModifier(pilotHigh, pilotHigh.Tactics, modifiersForSkill, new List<string>() { "AbilityDefT5A", "AbilityDefT8A" }, abilityMulti)
                );

            PilotDef pilotDefMed = new PilotDef(humanDescDef, 7, 6, 5, 4, 0, 3, false, 0, "voice",
                new List<string>() { "AbilityDefG5" }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotMed = new Pilot(pilotDefMed, "-1", false);
            Traverse.Create(pilotMed).Property("Abilities").SetValue(new List<Ability>());

            pilotMed.Abilities.Add(new Ability(gutsDef5));

            Assert.AreEqual(37,
                SkillUtils.GetModifier(pilotMed, pilotMed.Gunnery, modifiersForSkill, new List<string>() { "AbilityDefG5", "AbilityDefG8" }, abilityMulti)
                );
            Assert.AreEqual(30,
                SkillUtils.GetModifier(pilotMed, pilotMed.Piloting, modifiersForSkill, new List<string>() { "AbilityDefP5", "AbilityDefP8" }, abilityMulti)
                );
            Assert.AreEqual(25,
                SkillUtils.GetModifier(pilotMed, pilotMed.Guts, modifiersForSkill, new List<string>() { "AbilityDefGu5", "AbilityDefGu8" }, abilityMulti)
                );
            Assert.AreEqual(20,
                SkillUtils.GetModifier(pilotMed, pilotMed.Tactics, modifiersForSkill, new List<string>() { "AbilityDefT5A", "AbilityDefT8A" }, abilityMulti)
                );

            PilotDef pilotDefLog = new PilotDef(humanDescDef, 4, 3, 2, 1, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotLow = new Pilot(pilotDefLog, "-1", false);

            Assert.AreEqual(20,
                SkillUtils.GetModifier(pilotLow, pilotLow.Gunnery, modifiersForSkill, new List<string>() { "AbilityDefG5", "AbilityDefG8" }, abilityMulti)
                );
            Assert.AreEqual(15,
                SkillUtils.GetModifier(pilotLow, pilotLow.Piloting, modifiersForSkill, new List<string>() { "AbilityDefP5", "AbilityDefP8" }, abilityMulti)
                );
            Assert.AreEqual(10,
                SkillUtils.GetModifier(pilotLow, pilotLow.Guts, modifiersForSkill, new List<string>() { "AbilityDefGu5", "AbilityDefGu8" }, abilityMulti)
                );
            Assert.AreEqual(5,
                SkillUtils.GetModifier(pilotLow, pilotLow.Tactics, modifiersForSkill, new List<string>() { "AbilityDefT5A", "AbilityDefT8A" }, abilityMulti)
                );
        }

    }

}

