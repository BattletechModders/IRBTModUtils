using BattleTech;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using us.frostraptor.modUtils;

namespace IRBTModUtilsTests
{
    [TestFixture]
    public class SkillUtilsTests
    {

        [Test]
        public void LegacyGetModifier_Base()
        {
            HumanDescriptionDef humanDescDef = new HumanDescriptionDef();

            // gun, pilot, guts, tactics
            PilotDef pilotDefHigh = new PilotDef(humanDescDef, 10, 9, 8, 7, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotHigh = new Pilot(pilotDefHigh, "-1", false);

            Assert.Equals(5, SkillUtils.GetGunneryModifier(pilotHigh));
            Assert.Equals(4, SkillUtils.GetPilotingModifier(pilotHigh));
            Assert.Equals(4, SkillUtils.GetGutsModifier(pilotHigh));
            Assert.Equals(3, SkillUtils.GetTacticsModifier(pilotHigh));

            PilotDef pilotDefMed = new PilotDef(humanDescDef, 7, 6, 5, 4, 0, 3, false, 0, "voice",
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotMed = new Pilot(pilotDefMed, "-1", false);

            Assert.Equals(3, SkillUtils.GetGunneryModifier(pilotMed));
            Assert.Equals(3, SkillUtils.GetPilotingModifier(pilotMed));
            Assert.Equals(2, SkillUtils.GetGutsModifier(pilotMed));
            Assert.Equals(2, SkillUtils.GetTacticsModifier(pilotMed));

            PilotDef pilotDefLog = new PilotDef(humanDescDef, 4, 3, 2, 1, 0, 3, false, 0, "voice", 
                new List<string>() { }, AIPersonality.Undefined, 0, 0, 0);
            Pilot pilotLow = new Pilot(pilotDefLog, "-1", false);

            Assert.Equals(2, SkillUtils.GetGunneryModifier(pilotLow));
            Assert.Equals(1, SkillUtils.GetPilotingModifier(pilotLow));
            Assert.Equals(1, SkillUtils.GetGutsModifier(pilotLow));
            Assert.Equals(0, SkillUtils.GetTacticsModifier(pilotLow));
        }

    }

}

