using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("ba648cd4033740aa987af4b641d583de")]
public class CheckAbilityWeaponFamilyGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponFamily[] Families;

	protected override int GetBaseValue()
	{
		if (this.GetAbilityWeapon() == null)
		{
			return 0;
		}
		if (!Families.HasItem(this.GetAbilityWeapon().Blueprint.Family))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Family";
	}
}
