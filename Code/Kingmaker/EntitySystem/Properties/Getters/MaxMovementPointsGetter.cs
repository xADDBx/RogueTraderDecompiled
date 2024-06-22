using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("c8ec1a8f34944ea68f48a475ea7e5e60")]
public class MaxMovementPointsGetter : UnitPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Max MP of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return (int)base.CurrentEntity.CombatState.ActionPointsBlueMax;
	}
}
