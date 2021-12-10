using BattleTech;
using Harmony;
using IRBTModUtils.CustomInfluenceMap;
using IRBTModUtils.Extension;
using System;
using System.Collections.Generic;

namespace IRBTModUtils.CustomInfluenceMap
{
    public static class CustomFactors
    {
        public static void Register(string id, List<CustomInfluenceMapAllyFactor> influenceFactors)
        {
            Mod.Log.Info?.Write($"Registering {influenceFactors.Count} allyFactors from mod: {id}");
            ModState.CustomAllyFactors.AddRange(influenceFactors);
        }

        public static void Register(string id, List<CustomInfluenceMapHostileFactor> influenceFactors)
        {
            Mod.Log.Info?.Write($"Registering {influenceFactors.Count} hostfileFactors from mod: {id}");
            ModState.CustomHostileFactors.AddRange(influenceFactors);
        }

        public static void Register(string id, List<CustomInfluenceMapPositionFactor> influenceFactors)
        {
            Mod.Log.Info?.Write($"Registering {influenceFactors.Count} positionFactors from mod: {id}");
            ModState.CustomPositionFactors.AddRange(influenceFactors);
        }

        public static void Remove(string id, List<CustomInfluenceMapAllyFactor> influenceFactors)
        {
            Mod.Log.Info?.Write($"Registering {influenceFactors.Count} removed allyFactors from mod: {id}");
            ModState.RemovedAllyFactors.AddRange(influenceFactors);
        }

        public static void Remove(string id, List<CustomInfluenceMapHostileFactor> influenceFactors)
        {
            Mod.Log.Info?.Write($"Registering {influenceFactors.Count} removed hostileFactors from mod: {id}");
            ModState.RemovedHostileFactors.AddRange(influenceFactors);
        }

        public static void Remove(string id, List<CustomInfluenceMapPositionFactor> influenceFactors)
        {
            Mod.Log.Info?.Write($"Registering {influenceFactors.Count} removed positionFactors from mod: {id}");
            ModState.RemovedPositionFactors.AddRange(influenceFactors);
        }

        public static List<CustomInfluenceMapAllyFactor> GetCustomAllyFactors()
        {
            List<CustomInfluenceMapAllyFactor> factors = new List<CustomInfluenceMapAllyFactor>();
            factors.AddRange(ModState.CustomAllyFactors);
            return factors;
        }

        public static List<CustomInfluenceMapAllyFactor> GetRemovedAllyFactors()
        {
            List<CustomInfluenceMapAllyFactor> factors = new List<CustomInfluenceMapAllyFactor>();
            factors.AddRange(ModState.RemovedAllyFactors);
            return factors;
        }

        public static List<CustomInfluenceMapHostileFactor> GetCustomHostileFactors()
        {
            List<CustomInfluenceMapHostileFactor> factors = new List<CustomInfluenceMapHostileFactor>();
            factors.AddRange(ModState.CustomHostileFactors);
            return factors;
        }

        public static List<CustomInfluenceMapHostileFactor> GetRemovedHostileFactors()
        {
            List<CustomInfluenceMapHostileFactor> factors = new List<CustomInfluenceMapHostileFactor>();
            factors.AddRange(ModState.RemovedHostileFactors);
            return factors;
        }

        public static List<CustomInfluenceMapPositionFactor> GetCustomPositionFactors()
        {
            List<CustomInfluenceMapPositionFactor> factors = new List<CustomInfluenceMapPositionFactor>();
            factors.AddRange(ModState.CustomPositionFactors);
            return factors;
        }

        public static List<CustomInfluenceMapPositionFactor> GetRemovedPositionFactors()
        {
            List<CustomInfluenceMapPositionFactor> factors = new List<CustomInfluenceMapPositionFactor>();
            factors.AddRange(ModState.RemovedPositionFactors);
            return factors;
        }
    }
}
