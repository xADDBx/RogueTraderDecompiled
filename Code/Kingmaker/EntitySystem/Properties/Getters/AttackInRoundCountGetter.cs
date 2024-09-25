using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("8b598d75e29a452599a67c43ccc941bb")]
public class AttackInRoundCountGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.CombatState.AttackInRoundCount;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of attacks made by " + FormulaTargetScope.Current + " this round";
	}
}
