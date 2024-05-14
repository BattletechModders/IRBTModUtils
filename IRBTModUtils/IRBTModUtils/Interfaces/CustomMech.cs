using BattleTech;
using Localize;
using System.Collections.Generic;
using UnityEngine;

namespace IRBTModUtils
{
    public static class ICustomMechDebug
    {
        public static bool IS_DEAD_DEBUG = false;
    }
    public interface ICustomMech
    {
        bool isSquad { get; }
        bool isVehicle { get; }
        bool isQuad { get; }
        bool isTurret { get; }
        string UnitTypeName { get; }
        ICombatant carrier { get; }
        bool isMountedExternal { get; }
        bool isTemporaryDead { get; }
        bool isReallyDead { get; }
        void setTemporaryDead(bool value);
        HashSet<ICombatant> attachedInternally { get; }
        HashSet<ICombatant> attachedExternally { get; }
        HashSet<ArmorLocation> GetDFASelfDamageLocations();
        HashSet<ArmorLocation> GetLandmineDamageArmorLocations();
        HashSet<ArmorLocation> GetBurnDamageArmorLocations();
        Dictionary<ArmorLocation, int> GetHitTable(AttackDirection from);
        Dictionary<int, float> GetAOESpreadArmorLocations();
        List<int> GetAOEPossibleHitLocations(Vector3 attackPos);
        Text GetLongArmorLocation(ArmorLocation location);
        ArmorLocation GetAdjacentLocations(ArmorLocation location);
        Dictionary<ArmorLocation, int> GetClusterTable(ArmorLocation originalLocation, Dictionary<ArmorLocation, int> hitTable);
        Dictionary<ArmorLocation, int> GetHitTableCluster(AttackDirection from, ArmorLocation originalLocation);
        void AddLinkedActor(AbstractActor actor, Vector3 relativePosition, bool keepPosition);
    }
}