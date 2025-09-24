using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("fdeaa84a0d394b5e802f670865dd067c")]
public class CurrentWeaponBlueprintStatsGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IAbilityWeapon, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public enum WeaponStatType
	{
		Damage,
		MaxDamage,
		Penetration,
		DodgePenetration,
		AdditionalHitChance,
		Recoil,
		MaxDistance,
		MaxAmmo,
		RateOfFire
	}

	[SerializeField]
	private WeaponStatType StatType;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Current weapon " + StatType;
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		switch (StatType)
		{
		case WeaponStatType.Damage:
			return abilityWeapon.Blueprint.WarhammerDamage;
		case WeaponStatType.MaxDamage:
			return abilityWeapon.Blueprint.WarhammerMaxDamage;
		case WeaponStatType.Penetration:
			return abilityWeapon.Blueprint.WarhammerPenetration;
		case WeaponStatType.DodgePenetration:
			return abilityWeapon.Blueprint.DodgePenetration;
		case WeaponStatType.AdditionalHitChance:
			return abilityWeapon.Blueprint.AdditionalHitChance;
		case WeaponStatType.Recoil:
			return abilityWeapon.Blueprint.WarhammerRecoil;
		case WeaponStatType.MaxDistance:
			return abilityWeapon.Blueprint.WarhammerMaxDistance;
		case WeaponStatType.MaxAmmo:
			return abilityWeapon.Blueprint.WarhammerMaxAmmo;
		case WeaponStatType.RateOfFire:
			return abilityWeapon.Blueprint.RateOfFire;
		default:
			PFLog.Ability.Error($"Wrong stat type in FirstWeaponStatGetter : {StatType}");
			return 0;
		}
	}
}
