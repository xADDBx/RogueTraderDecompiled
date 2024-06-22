using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("323b0da7a07e4dc7b3e5311d3c609ff6")]
public class CheckAbilityWeaponCategoryGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponCategory[] Categories;

	protected override int GetBaseValue()
	{
		if (this.GetAbilityWeapon() == null)
		{
			return 0;
		}
		if (!Categories.HasItem(this.GetAbilityWeapon().Blueprint.Category))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon Category";
	}
}
