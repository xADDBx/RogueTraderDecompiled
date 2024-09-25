using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("5f00e81fb0604d8a952703ec63ba2b7a")]
public class MovementPointsSpentThisTurnGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return Mathf.RoundToInt(base.CurrentEntity.CombatState.ActionPointsBlueSpentThisTurn);
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " MP spent this turn";
	}
}
