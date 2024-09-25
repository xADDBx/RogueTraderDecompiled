using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

[Serializable]
[TypeId("ea316121d4594ee6be95f23582aa6f10")]
public class AbilityUnrestrictedRangeForTarget : BlueprintComponent
{
	public PropertyCalculator TargetCondition;

	public bool IsRangeUnrestrictedForTarget(AbilityData ability, TargetWrapper target)
	{
		if (target.Entity == null)
		{
			return false;
		}
		PropertyContext context = new PropertyContext(ability.Caster, null, target.Entity).WithAbility(ability);
		return TargetCondition.GetBoolValue(context);
	}
}
