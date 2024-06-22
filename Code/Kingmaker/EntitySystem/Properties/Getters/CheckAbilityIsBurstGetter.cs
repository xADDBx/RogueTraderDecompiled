using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("402f67d675fd4705bed57b358df0798d")]
public class CheckAbilityIsBurstGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		AbilityData ability = this.GetAbility();
		if (ability == null)
		{
			return 0;
		}
		WarhammerAbilityAttackDelivery component = ability.Blueprint.GetComponent<WarhammerAbilityAttackDelivery>();
		if (component == null || !component.IsBurst)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if ability is burst shot";
	}
}
