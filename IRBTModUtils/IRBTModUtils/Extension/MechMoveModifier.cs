using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.Extension
{
    // Class representing additive, constant modifiers to a Mech's movement. When 
    //   calculating the current run or walk speed these modifiers will be applied.
    //   IRBTModUtils will scan for derived classes during the ModTek FinishedLoading() callback
    public abstract class MechMoveModifier
    {
        public abstract float WalkSpeedModifier(Mech mech);
        public abstract float RunSpeedModifier(Mech mech);
      
    }
}
