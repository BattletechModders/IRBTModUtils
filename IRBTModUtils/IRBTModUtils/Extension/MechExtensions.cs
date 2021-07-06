using BattleTech;
using System;
using System.Collections.Generic;

namespace IRBTModUtils.Extension
{
    public static class MechExtensions
    {
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
        public static float ModifiedWalkDistance(this Mech mech) {
          return ModifiedWalkDistanceExt(mech, true);
        }
        public static float ModifiedRunDistance(this Mech mech) {
          return ModifiedRunDistanceExt(mech, true);
        }
        public static float ModifiedWalkDistanceExt(this Mech mech, bool skipExternalAll, params string[] without)
        {
            Mod.Log.Info?.Write($"ModifiedWalkDistance for: {mech.DistinctId()} extMods: {extMoveMods.Count}");
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
            if (skipExternalAll == false) {
              HashSet<string> skipMods = new HashSet<string>();
              if(without != null) {
                if (without.Length > 0) { foreach (string w in without) { skipMods.Add(w); } }
              }
              foreach (MechMoveDistanceModifier extmod in extMoveMods) {
                if (extmod.walkMod == null) { continue; }
                if (skipMods.Contains(extmod.name)) { continue; }
                Mod.Log.Info?.Write($" ext modifier: {extmod.name} {walkDistance}");
                walkDistance = extmod.walkMod(mech, walkDistance);
              }
            }
            return walkDistance;
        }

        public static float ModifiedRunDistanceExt(this Mech mech, bool skipExternalAll, params string[] without)
        {
            Mod.Log.Info?.Write($"ModifiedRunDistance for: {mech.DistinctId()} extMods: {extMoveMods.Count}");
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
            if (skipExternalAll == false) {
              HashSet<string> skipMods = new HashSet<string>();
              if (without != null) {
                if (without.Length > 0) { foreach (string w in without) { skipMods.Add(w); } }
              }
              foreach (MechMoveDistanceModifier extmod in extMoveMods) {
                if (extmod.runMod == null) { continue; }
                if (skipMods.Contains(extmod.name)) { continue; }
                Mod.Log.Info?.Write($" ext modifier: {extmod.name} {runDistance}");
                runDistance = extmod.runMod(mech, runDistance);
              }
            }
            return runDistance;
        }

    }
}
