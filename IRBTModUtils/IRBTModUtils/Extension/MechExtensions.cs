using BattleTech;
using System;
using System.Collections.Generic;

namespace IRBTModUtils.Extension
{
    public static class MechExtensions
    {
        private static List<Func<Mech, float, float>> extWalkMods = new List<Func<Mech, float, float>>();
        private static List<Func<Mech, float, float>> extRunMods = new List<Func<Mech, float, float>>();
        public static void AddExternalWalkModifier(Func<Mech, float, float> mod) { extWalkMods.Add(mod); }
        public static void AddExternalRunModifier(Func<Mech, float, float> mod) { extRunMods.Add(mod); }
        public static float ModifiedWalkDistance(this Mech mech) {
          return ModifiedWalkDistanceExt(mech, true);
        }
        public static float ModifiedRunDistance(this Mech mech) {
          return ModifiedRunDistanceExt(mech, true);
        }
        public static float ModifiedWalkDistanceExt(this Mech mech, bool extModifiers = true)
        {
            Mod.Log.Info?.Write($"ModifiedWalkDistance for: {mech.DistinctId()}");
            float walkDistance = 0f;
            if (mech == null) { return walkDistance; }

            float total = 0f;
            foreach (MechMoveModifier moveModifier in ModState.MoveModifiers)
            {
                total += moveModifier.WalkSpeedModifier(mech);
            }

            walkDistance = (float)Math.Ceiling(mech.WalkSpeed - total);
            if (walkDistance < Mod.Config.MinimumMove)
                walkDistance = Mod.Config.MinimumMove;
            if(extModifiers) foreach (Func<Mech, float, float> extmod in extWalkMods) { walkDistance = extmod(mech, walkDistance); }
            return walkDistance;
        }

        public static float ModifiedRunDistanceExt(this Mech mech, bool extModifiers = true)
        {
            Mod.Log.Info?.Write($"ModifiedRunDistance for: {mech.DistinctId()}");
            float runDistance = 0f;
            if (mech == null) { return runDistance; }

            float total = 0f;
            foreach (MechMoveModifier moveModifier in ModState.MoveModifiers)
            {
                total += moveModifier.RunSpeedModifier(mech);
            }

            runDistance = (float)Math.Ceiling(mech.RunSpeed - total);
            if (runDistance < Mod.Config.MinimumMove)
                runDistance = Mod.Config.MinimumMove;
            if(extModifiers) foreach (Func<Mech, float, float> extmod in extRunMods) { runDistance = extmod(mech, runDistance); }
            return runDistance;
        }

    }
}
