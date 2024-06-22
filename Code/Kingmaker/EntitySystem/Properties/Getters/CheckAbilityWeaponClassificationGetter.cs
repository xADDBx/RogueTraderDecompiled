using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("d5a557904cf7461f90f423f098160e66")]
public class CheckAbilityWeaponClassificationGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public WeaponClassification[] Classifications;

	protected override int GetBaseValue()
	{
		if (this.GetAbilityWeapon() == null)
		{
			return 0;
		}
		if (!Classifications.HasItem(this.GetAbilityWeapon().Blueprint.Classification))
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
