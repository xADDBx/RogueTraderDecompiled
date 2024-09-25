using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("8b734b0a49094c9f8957d90790e3d8fb")]
public class AttackedInRoundCountGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.CombatState.AttackedInRoundCount;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of attacks suffered by " + FormulaTargetScope.Current + " this round";
	}
}
