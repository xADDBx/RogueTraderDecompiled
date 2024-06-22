using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("90e37f9c2bb670b4bb0adf1e9eadccde")]
public class CurrentWeaponBlueprintDamageGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IAbilityWeapon, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public enum WeaponDamage
	{
		Min,
		Max
	}

	public WeaponDamage Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Current {Type} weapon damage";
	}

	protected override int GetBaseValue()
	{
		return Type switch
		{
			WeaponDamage.Min => this.GetAbilityWeapon().Blueprint.WarhammerDamage, 
			WeaponDamage.Max => this.GetAbilityWeapon().Blueprint.WarhammerMaxDamage, 
			_ => 0, 
		};
	}
}
