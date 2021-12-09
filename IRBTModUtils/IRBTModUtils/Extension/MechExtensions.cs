using BattleTech;
using IRBTModUtils.Feature;
using System;

namespace IRBTModUtils.Extension
{
    public static class MechExtensions
    {

        [Obsolete("Use IRBTModUtils.Feature.MovementFeature.RegisterMoveDistanceModifier instead")]
        public static void RegisterMoveDistanceModifier(string id, int priority, Func<Mech, float, float> walkmod, Func<Mech, float, float> runmod) 
        {
            MovementFeature.RegisterMoveDistanceModifier(id, priority, walkmod, runmod);
        }

        // Extension points for KMission
        public static float ModifiedWalkDistanceExt(this Mech mech, bool skipExternalAll, params string[] without)
        {
            return Mod.Config.Features.EnableMovementModifiers ?
                MovementFeature.ModifiedDistanceExt(mech, skipExternalAll, isRun: false, without) : mech.WalkSpeed;
        }

        public static float ModifiedRunDistanceExt(this Mech mech, bool skipExternalAll, params string[] without)
        {
            return Mod.Config.Features.EnableMovementModifiers ? 
                MovementFeature.ModifiedDistanceExt(mech, skipExternalAll, isRun: true, without) : mech.RunSpeed;
        }

        

    }
}
