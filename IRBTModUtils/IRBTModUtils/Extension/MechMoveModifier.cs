using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.Extension
{
    public abstract class MechMoveModifier
    {
        public abstract float WalkSpeedModifier(Mech mech);
        public abstract float RunSpeedModifier(Mech mech);
      
    }
}
