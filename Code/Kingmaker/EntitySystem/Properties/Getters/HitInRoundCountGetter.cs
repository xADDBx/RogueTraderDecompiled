using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("bbd2c9ef81d244bfacc4bf7282799323")]
public class HitInRoundCountGetter : UnitPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.CombatState.HitInRoundCount;
	}

	protected override string GetInnerCaption()
	{
		return "HitInRoundCount";
	}
}
