using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("53ddc60b996d4bd2aa81e66b49d5f97d")]
public class CheckAbilityWeaponRangeTypeGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	private enum WeaponRangeType
	{
		Melee,
		Ranged
	}

	[SerializeField]
	private WeaponRangeType m_RangeType;

	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		if (abilityWeapon == null)
		{
			return 0;
		}
		bool isMelee = abilityWeapon.Blueprint.IsMelee;
		if ((isMelee && m_RangeType == WeaponRangeType.Melee) || (!isMelee && m_RangeType == WeaponRangeType.Ranged))
		{
			return 1;
		}
		return 0;
	}

	protected override string GetInnerCaption()
	{
		string text = ((m_RangeType == WeaponRangeType.Melee) ? "Melee" : "Ranged");
		return "Ability Weapon Range is " + text;
	}
}
