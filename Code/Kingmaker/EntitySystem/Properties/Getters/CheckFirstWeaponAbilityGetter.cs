using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("4d6a30178423cf44aa80c44e2f8573e8")]
public class CheckFirstWeaponAbilityGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalAbilityWeapon
{
	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		BlueprintAbility blueprintAbility = this.GetAbility()?.Blueprint;
		if (abilityWeapon == null || blueprintAbility == null)
		{
			return 0;
		}
		if (abilityWeapon.Blueprint.WeaponAbilities.Ability1.Ability != blueprintAbility)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability is first weapon ability";
	}
}
