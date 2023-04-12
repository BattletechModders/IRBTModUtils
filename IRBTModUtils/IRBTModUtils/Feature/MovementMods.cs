using HBS.Collections;
using IRBTModUtils.Extension;
using System;
using System.Collections.Generic;

namespace IRBTModUtils.Feature
{
    public static class MovementFeature
    {
        public static void RegisterMoveDistanceModifier(string id, int priority, Func<Mech, float, float> walkmod, Func<Mech, float, float> runmod)
        {
            ModState.ExtMovementMods.Add(new MechMoveDistanceModifier(id, walkmod, runmod, priority));
            ModState.ExtMovementMods.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        internal static float ModifiedDistanceExt(this Mech mech, bool skipExternalAll, bool isRun = false, params string[] extensionsToSkip)
        {
            if (mech == null) { return 0f; }

            TagSet mechTags = mech.GetTags();
            if (mechTags.Contains(ModTags.ImmobileUnit))
            {
                Mod.Log.Debug?.Write($"Mech: {mech.DistinctId()} has tag: '{ModTags.ImmobileUnit}', returning 0 movement.");
                return 0;
            }

            bool isImmobile = mech.StatCollection.GetValue<bool>(ModStats.ImmobileUnit);
            if (isImmobile)
            {
                Mod.Log.Debug?.Write($"Mech: {mech.DistinctId()} has statistic: '{ModStats.ImmobileUnit}' set to true, returning 0 movement.");
                return 0;
            }

            float modifiedDist = isRun ? mech.RunSpeed : mech.WalkSpeed;
            try
            {
                string typeLabel = isRun ? "ModifiedRunDistance" : "ModifiedWalkDistance";
                Mod.Log.Debug?.Write($"Calc {typeLabel} for: {mech.DistinctId()} (extModsCount: {ModState.ExtMovementMods.Count})");

                float baseMoveMulti = mech.MoveMultiplier;
                modifiedDist *= baseMoveMulti;
                Mod.Log.Debug?.Write($"  -- stat 'MoveMultiplier' = {baseMoveMulti} => modifiedDist: {modifiedDist}");

                if (!skipExternalAll)
                {
                    // Filter mods that should be excluded from the comparison
                    HashSet<string> skipMods = new HashSet<string>();
                    if (extensionsToSkip != null)
                    {
                        if (extensionsToSkip.Length > 0) { foreach (string w in extensionsToSkip) { skipMods.Add(w); } }
                    }

                    foreach (MechMoveDistanceModifier extmod in ModState.ExtMovementMods)
                    {
                        if (skipMods.Contains(extmod.Name)) { continue; }
                        //if (extmod.WalkMod == null) { continue; }

                        float previousDist = modifiedDist;
                        modifiedDist = isRun ? extmod.RunMod(mech, modifiedDist) : extmod.WalkMod(mech, modifiedDist);
                        Mod.Log.Debug?.Write($" -- ext modifier: {extmod.Name} => before: {previousDist} after: {modifiedDist}");
                    }
                }

                // Make comparisons safer
                modifiedDist = (float)Math.Ceiling(modifiedDist);

                if (modifiedDist < Mod.Config.MinimumMove)
                {
                    Mod.Log.Debug?.Write($"  -- calculated move dist: {modifiedDist} < minMove: {Mod.Config.MinimumMove}, returning minMove");
                    modifiedDist = Mod.Config.MinimumMove;
                }

                bool speedChanged = true;
                string cacheKey = mech.DistinctId() + (isRun ? "_r" : "_w");
                if (SharedState.MechSpeedCache.TryGetValue(cacheKey, out float cachedDistance))
                {
                    if (cachedDistance == modifiedDist) speedChanged = false;
                }

                if (speedChanged)
                {
                    Mod.Log.Info?.Write($"{typeLabel} changed for '{mech.DistinctId()}' to {modifiedDist}m " +
                        $"from: {(isRun ? mech.RunSpeed : mech.WalkSpeed)}m isRun: {isRun}");
                    SharedState.MechSpeedCache[cacheKey] = modifiedDist;
                }
                else
                {
                    Mod.Log.Trace?.Write($" cached speed value unchanged, skipping.");
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

    // Class representing dynamic movement modifiers that are not constant or that require the current state of the mech
    //   to calculate. These are used in conjunction with MechMoveModifiers to calculate the final walk and run speeds
    internal class MechMoveDistanceModifier
    {
        public string Name;

        // Parameters:
        //    mech to be updated
        //    current, MODIFIED value for the unit
        //    updated value for the unit
        public Func<Mech, float, float> WalkMod { get; private set; }
        public Func<Mech, float, float> RunMod { get; private set; }
        public int Priority { get; private set; }

        public MechMoveDistanceModifier(string id, Func<Mech, float, float> walkMod, Func<Mech, float, float> runMod, int priority)
        {
            Name = id;
            this.WalkMod = walkMod;
            this.RunMod = runMod;
            this.Priority = priority;
        }
    }

   
}
