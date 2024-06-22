using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("0ab3ba348c2e46ff9a6623fce1e3ac84")]
public class AbilityWeaponBlueprintRateOfFireGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		return this.GetAbilityWeapon()?.Blueprint.RateOfFire ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Unmodified Rate of Fire (from blueprint)";
	}
}
