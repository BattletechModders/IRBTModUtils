using BattleTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRBTModUtils.CustomInfluenceMap
{
	public abstract class CustomInfluenceMapHostileFactor : InfluenceMapHostileFactor
	{
		public override string Name => "ALWAYS OVERRIDE ME!";

		// We always return INVALID_UNSET because most custom behaviors will use unique behavior variables or calcluations
		public override BehaviorVariableName GetRegularMoveWeightBVName() { return BehaviorVariableName.INVALID_UNSET; }

		public override BehaviorVariableName GetSprintMoveWeightBVName() { return BehaviorVariableName.INVALID_UNSET; }

		public abstract float GetRegularMoveWeight(AbstractActor actor);
		public abstract float GetSprintMoveWeight(AbstractActor actor);

	}
}
