using BattleTech;
using System;

namespace IRBTModUtils.Extension
{
    public static class MechExtensions
    {
        public static float ModifiedWalkDistance(this Mech mech)
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

            return walkDistance;
        }

        public static float ModifiedRunDistance(this Mech mech)
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

            return runDistance;
        }

    }
}
