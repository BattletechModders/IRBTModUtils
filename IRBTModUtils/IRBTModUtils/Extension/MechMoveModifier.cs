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
        public abstract float ModifyWalkSpeed(Mech mech);
        public abstract float ModifyRunSpeed(Mech mech);
        public abstract float ModifyJumpSpeed(Mech mech);
    }
}
