using BattleTech;

namespace IRBTModUtils.Extension
{
    public static class MechExtensions
    {
        public static float ModifiedWalkDistance(this Mech mech) 
        {
            float walkDistance = 0f;
            if (mech == null) { return walkDistance; }

            float total = 0f;
            foreach (MechMoveModifier moveModifier in ModState.MoveModifiers)
            {
                total = moveModifier.ModifyWalkSpeed(mech);
            }

            walkDistance = mech.WalkSpeed - total;
            if (walkDistance < Mod.Config.MinimumMove)
                walkDistance = Mod.Config.MinimumMove;

            return walkDistance;
        }

        public static float ModifiedRunDistance(this Mech mech)
        {
            float runDistance = 0f;
            if (mech == null) { return runDistance; }

            float total = 0f;
            foreach (MechMoveModifier moveModifier in ModState.MoveModifiers)
            {
                total = moveModifier.ModifyRunSpeed(mech);
            }

            runDistance = mech.RunSpeed - total;
            if (runDistance < Mod.Config.MinimumMove)
                runDistance = Mod.Config.MinimumMove;

            return runDistance;
        }

        public static float ModifiedJumpDistance(this Mech mech)
        {
            float jumpDistance = 0f;
            if (mech == null) { return jumpDistance; }

            float total = 0f;
            foreach (MechMoveModifier moveModifier in ModState.MoveModifiers)
            {
                total = moveModifier.ModifyJumpSpeed(mech);
            }

            jumpDistance = mech.JumpDistance - total;
            if (jumpDistance < Mod.Config.MinimumJump)
                jumpDistance = Mod.Config.MinimumJump;

            return jumpDistance;
        }
    }
}
