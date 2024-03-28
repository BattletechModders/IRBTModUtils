using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.Interfaces
{
    public interface ICustomMechDef
    {
        bool isSquad { get; }
        bool isVehicle { get; }
        bool isQuad { get; }
        bool isTurret { get; }
        string UnitTypeName { get; }
        Dictionary<ChassisLocations, string> locationNames { get; }
    }
}
