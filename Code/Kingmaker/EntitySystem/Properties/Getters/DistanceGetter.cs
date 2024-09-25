using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("8cc2fc2a697041af82344696abdf3c4d")]
public class DistanceGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		Vector3 targetPositionByType = this.GetTargetPositionByType(Target);
		IntRect targetRectByType = this.GetTargetRectByType(Target);
		return base.CurrentEntity.DistanceToInCells(targetPositionByType, targetRectByType);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Distance from " + FormulaTargetScope.Current + " to " + Target.Colorized();
	}
}
