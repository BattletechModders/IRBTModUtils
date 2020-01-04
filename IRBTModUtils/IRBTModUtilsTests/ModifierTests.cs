using NUnit.Framework;
using us.frostraptor.modUtils.math;

namespace HexUtilsModifiersTests {

    [TestFixture]
    public class DecayingAttackTests {

        [Test]
        public void ZoomMod() {
            // 0 to 3 over 3 steps

            // No range, no modifier
            int mod = HexUtils.DecayingModifier(0, -3, 3, 0.0f);
            Assert.AreEqual(0, mod);

            // Range just under step count of 3 * 30 = 89.9
            mod = HexUtils.DecayingModifier(0, -3, 3, 89.9f);
            Assert.AreEqual(0, mod);

            // Range at first step count
            mod = HexUtils.DecayingModifier(0, -3, 3, 90.0f);
            Assert.AreEqual(-1, mod);

            // Range at second step count
            mod = HexUtils.DecayingModifier(0, -3, 3, 180.0f);
            Assert.AreEqual(-2, mod);

            // Range at third step count
            mod = HexUtils.DecayingModifier(0, -3, 3, 270.0f);
            Assert.AreEqual(-3, mod);

            // Range at fourth step count
            mod = HexUtils.DecayingModifier(0, -3, 3, 360.0f);
            Assert.AreEqual(-3, mod);
        }

        [Test]
        public void BrawlerMod() {
            // -3 to +3 over 6 steps

            // No range, full modifier
            int mod = HexUtils.DecayingModifier(-3, 3, 2, 0.0f);
            Assert.AreEqual(-3, mod);

            // First step (2*1*30), worse mod
            mod = HexUtils.DecayingModifier(-3, 3, 2, 60.0f);
            Assert.AreEqual(-2, mod);

            // Third step (2*3*30), neutral
            mod = HexUtils.DecayingModifier(-3, 3, 2, 180.0f);
            Assert.AreEqual(0, mod);

            // Fourth step (2*4*30), positive
            mod = HexUtils.DecayingModifier(-3, 3, 2, 240.0f);
            Assert.AreEqual(1, mod);

            // Sixth step (2*6*30), positive
            mod = HexUtils.DecayingModifier(-3, 3, 2, 360.0f);
            Assert.AreEqual(3, mod);

            // Seventh step (2*7*30), capped
            mod = HexUtils.DecayingModifier(-3, 3, 2, 420.0f);
            Assert.AreEqual(3, mod);
        }

        [Test]
        public void SniperMod() {
            // +3 to -3 over 6 steps

            // No range, full modifier
            int mod = HexUtils.DecayingModifier(3, -3, 2, 0.0f);
            Assert.AreEqual(3, mod);

            // First step (2*1*30), worse mod
            mod = HexUtils.DecayingModifier(3, -3, 2, 60.0f);
            Assert.AreEqual(2, mod);

            // Third step (2*3*30), neutral
            mod = HexUtils.DecayingModifier(3, -3, 2, 180.0f);
            Assert.AreEqual(0, mod);

            // Fourth step (2*4*30), positive
            mod = HexUtils.DecayingModifier(3, -3, 2, 240.0f);
            Assert.AreEqual(-1, mod);

            // Sixth step (2*6*30), positive
            mod = HexUtils.DecayingModifier(3, -3, 2, 360.0f);
            Assert.AreEqual(-3, mod);

            // Seventh step (2*7*30), capped
            mod = HexUtils.DecayingModifier(3, -3, 2, 420.0f);
            Assert.AreEqual(-3, mod);

        }

        // === Modiifers used by LA in RT ===
        [Test]
        public void LAMod() {
            int initialMod = 0, finalMod = -1, hexSteps = 10;

            // No range, no modifier
            int mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 0f);
            Assert.AreEqual(0, mod);

            // Not full step, no modifier
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 200f);
            Assert.AreEqual(0, mod);

            // One full step, modifier
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 300f);
            Assert.AreEqual(-1, mod);

            // Two steps, same modifier
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 600f);
            Assert.AreEqual(-1, mod);
        }

        [Test]
        public void LAMod2() {
            int initialMod = -4, finalMod = 0, hexSteps = 4;

            // No range, no modifier
            int mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 0f);
            Assert.AreEqual(-4, mod);

            // Not full step, no modifier
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 110f);
            Assert.AreEqual(-4, mod);

            // One step
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 125f);
            Assert.AreEqual(-3, mod);

            // Two steps
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 245f);
            Assert.AreEqual(-2, mod);

            // Three steps
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 365f);
            Assert.AreEqual(-1, mod);

            // Four steps, same modifier
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 485f);
            Assert.AreEqual(-0, mod);

            // Five steps, same modifier
            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 610f);
            Assert.AreEqual(0, mod);
        }


        // === Modifiers used by Harkonnen in his NAOP mod ===
        [Test]
        public void HarkonnenMod() {
            int initialMod = 0, finalMod = 5, hexSteps = 2;
            int mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 0f);
            Assert.AreEqual(0, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 60f);
            Assert.AreEqual(1, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 120f);
            Assert.AreEqual(2, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 180f);
            Assert.AreEqual(3, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 240f);
            Assert.AreEqual(4, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 300f);
            Assert.AreEqual(5, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 360f);
            Assert.AreEqual(5, mod);

            mod = HexUtils.DecayingModifier(initialMod, finalMod, hexSteps, 500f);
            Assert.AreEqual(5, mod);
        }

    }
}
