using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3d3ea03bc11a4a68ba661ee1b53388a8")]
public class CheckAbilityIsWeaponAbilityGetter : UnitPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool OnlyHasAttackDelivery;

	protected override int GetBaseValue()
	{
		if (OnlyHasAttackDelivery && base.PropertyContext.Ability != null)
		{
			if (!base.PropertyContext.Ability.Blueprint.ComponentsArray.Contains((BlueprintComponent p) => p is WarhammerAbilityAttackDelivery))
			{
				return 0;
			}
			return 1;
		}
		if (this.GetAbilityWeapon() == null)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability comes from a weapon or uses a weapon";
	}
}
