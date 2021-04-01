using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.CustomInfluenceMap
{
    // A container that indicates a mod wants to remove a factor from the vanilla InfluenceMap. 
    //   This will be read by CleverGirl at mod startup, and any types listed will be removed from all units.
    public abstract class InfluenceMapFactorsToRemove
    {
        public abstract IEnumerable<InfluenceMapAllyFactor> AllyFactorsToRemove();
        public abstract IEnumerable<InfluenceMapHostileFactor> HostileFactorsToRemove();
        public abstract IEnumerable<InfluenceMapPositionFactor> PositionFactorsToRemove();
    }
}
