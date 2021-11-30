using BattleTech;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace IRBTModUtils.Extension
{
    public static class MechExtensions
    {
        // Class representing dynamic movement modifiers that are not constant or that require the current state of the mech
        //   to calculate. These are used in conjunction with MechMoveModifiers to calculate the final walk and run speeds
        internal class MechMoveDistanceModifier {
          public string name;
          public Func<Mech, float, float> walkMod { get; private set; }
          public Func<Mech, float, float> runMod { get; private set; }
          public int priority { get; private set; }

          public MechMoveDistanceModifier(string id, Func<Mech, float, float> walkMod, Func<Mech, float, float> runMod, int priority) {
            this.name = id;
            this.walkMod = walkMod;
            this.runMod = runMod;
            this.priority = priority;
          }
        }

        private static List<MechMoveDistanceModifier> extMoveMods = new List<MechMoveDistanceModifier>();

        public static void RegisterMoveDistanceModifier(string id, int priority, Func<Mech, float, float> walkmod, Func<Mech, float, float> runmod) {
          extMoveMods.Add(new MechMoveDistanceModifier(id, walkmod, runmod, priority));
          extMoveMods.Sort((x, y) => x.priority.CompareTo(y.priority));
        }

        // Maintain for CBTBE compat (until after upgrade)
        [Obsolete("Use ModifiedWalkDistanceExt instead")]
        public static float ModifiedWalkDistance(this Mech mech)
        {
            return ModifiedDistanceExt(mech, skipExternalAll: true, isRun: false);
        }
        [Obsolete("Use ModifiedRunDistanceExt instead")]
        public static float ModifiedRunDistance(this Mech mech)
        {
            return ModifiedDistanceExt(mech, skipExternalAll: true, isRun: true);
        }

        // Extension points for KMission
        public static float ModifiedWalkDistanceExt(this Mech mech, bool skipExternalAll, params string[] without)
        {
            return ModifiedDistanceExt(mech, skipExternalAll, isRun: false, without);
        }

        public static float ModifiedRunDistanceExt(this Mech mech, bool skipExternalAll, params string[] without)
        {
            return ModifiedDistanceExt(mech, skipExternalAll, isRun: true, without);
        }

        private static float ModifiedDistanceExt(this Mech mech, bool skipExternalAll, bool isRun = false, params string[] without)
        {
            if (mech == null) { return 0f; }

            float modifiedDist = 0;
            try
            {
                string typeLabel = isRun ? "ModifiedRunDistance" : "ModifiedWalkDistance";
                Mod.Log.Debug?.Write($"Calc {typeLabel} for: {mech.DistinctId()} " +
                    $"(extModsCount: {extMoveMods.Count} moveModsCount: {ModState.MoveModifiers.Count})");

                // Apply static, additive modifiers to movement
                float total = 0f;
                foreach (MechMoveModifier moveModifier in ModState.MoveModifiers)
                {
                    total += isRun ? moveModifier.RunSpeedModifier(mech) : moveModifier.WalkSpeedModifier(mech);
                }
                modifiedDist = (float)Math.Ceiling((isRun ? mech.RunSpeed : mech.WalkSpeed) - total);

                // Apply dynamic modifiers to movement
                if (!skipExternalAll)
                {
                    // Filter mods that should be excluded from the comparison
                    HashSet<string> skipMods = new HashSet<string>();
                    if (without != null)
                    {
                        if (without.Length > 0) { foreach (string w in without) { skipMods.Add(w); } }
                    }

                    foreach (MechMoveDistanceModifier extmod in extMoveMods)
                    {
                        if (extmod.walkMod == null) { continue; }
                        if (skipMods.Contains(extmod.name)) { continue; }

                        float beforeChange = modifiedDist;
                        modifiedDist = isRun ? extmod.runMod(mech, modifiedDist) : extmod.walkMod(mech, modifiedDist);
                        Mod.Log.Debug?.Write($" ext modifier: {extmod.name} beforeChange: {modifiedDist} afterChange: {modifiedDist}");
                    }
                }

                // Make comparisons safer
                modifiedDist = Mathf.RoundToInt(modifiedDist);

                if (modifiedDist < Mod.Config.MinimumMove) modifiedDist = Mod.Config.MinimumMove;

                bool speedChanged = true;
                string cacheKey = mech.DistinctId() + (isRun ? "_r" : "_w");
                if (SharedState.MechSpeedCache.TryGetValue(cacheKey, out float cachedDistance))
                    if (cachedDistance == modifiedDist) speedChanged = false;

                if (speedChanged)
                {
                    Mod.Log.Info?.Write($"{typeLabel} changed for '{mech.DistinctId()}' to {modifiedDist}m from: {(isRun ? mech.RunSpeed : mech.WalkSpeed)}m " +
                        $"isRun: {isRun} (extModsCount: {extMoveMods.Count} moveModsCount: {ModState.MoveModifiers.Count})");
                    SharedState.MechSpeedCache[cacheKey] = modifiedDist;
                }
                else
                {
                    Mod.Log.Debug?.Write($" cached speed value unchanged, skipping.");
                }
            }
            catch (Exception e)
            {
                Mod.Log.Warn?.Write(e, $"Failed to calculate modified move/run speed!");
                modifiedDist = isRun ? mech.RunSpeed : mech.WalkSpeed;
            }

            return modifiedDist;
        }

    }
}
