using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("24ff12aa2865ed7449fa2a5b268a8e5b")]
public class CurrentMovementPointsGetter : UnitPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "MP of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.CombatState.ActionPointsBlueMax - base.CurrentEntity.CombatState.ActionPointsBlueSpentThisTurn);
	}
}
