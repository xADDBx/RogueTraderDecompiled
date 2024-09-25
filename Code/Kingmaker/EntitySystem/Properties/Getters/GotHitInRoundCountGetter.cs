using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("6de5af7490cf4d5d8415dcb778ce04e1")]
public class GotHitInRoundCountGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.CombatState.GotHitInRoundCount;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of hits on " + FormulaTargetScope.Current + " this round";
	}
}
