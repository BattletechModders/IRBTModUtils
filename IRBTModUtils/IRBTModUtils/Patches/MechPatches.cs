using BattleTech;
using Harmony;
using IRBTModUtils.Extension;

namespace IRBTModUtils.Patches
{

    /* Override walk speed. Reference also: 
     *   MechEngineer.Features.ShutdownInjuryProtection
     *   MechEngineer.Features.Engine
     */
    [HarmonyPatch(typeof(Mech))]
    [HarmonyPatch("MaxWalkDistance", MethodType.Getter)]
    [HarmonyBefore("io.mission.customunits")]
    public static class Mech_MaxWalkDistance_Get
    {
        static bool Prepare() => Mod.Config.Features.EnableMovementModifiers;

        public static void Postfix(Mech __instance, ref float __result)
        {
            if (SharedState.Combat != null)
                __result = __instance.ModifiedWalkDistanceExt(false);
        }
    }

    [HarmonyPatch(typeof(Mech))]
    [HarmonyPatch("MaxBackwardDistance", MethodType.Getter)]
    [HarmonyBefore("io.mission.customunits")]
    public static class Mech_MaxBackwardDistance_Get
    {
        static bool Prepare() => Mod.Config.Features.EnableMovementModifiers;

        public static void Postfix(Mech __instance, ref float __result)
        {
            if (SharedState.Combat != null)
                __result = __instance.ModifiedWalkDistanceExt(false);
        }
    }

    [HarmonyPatch(typeof(Mech))]
    [HarmonyPatch("MaxSprintDistance", MethodType.Getter)]
    [HarmonyBefore("io.mission.customunits")]
    public static class Mech_MaxSprintDistance_Get
    {
        static bool Prepare() => Mod.Config.Features.EnableMovementModifiers;

        public static void Postfix(Mech __instance, ref float __result)
        {
            if (SharedState.Combat != null)
                __result = __instance.ModifiedRunDistanceExt(false);
        }
    }
}
