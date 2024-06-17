using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("b686e6d4f01af2244bdef9f165f2c511")]
public class CheckAbilityWeaponChosenGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public bool SpecificWeapon;

	[SerializeField]
	[ShowIf("SpecificWeapon")]
	private BlueprintItemWeaponReference[] m_Weapons;

	public ReferenceArrayProxy<BlueprintItemWeapon> Weapons
	{
		get
		{
			BlueprintReference<BlueprintItemWeapon>[] weapons = m_Weapons;
			return weapons;
		}
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		ItemEntityWeapon itemEntityWeapon = base.CurrentEntity.GetOptional<WarhammerUnitPartChooseWeapon>()?.ChosenWeapon;
		if (abilityWeapon == null || itemEntityWeapon == null)
		{
			return 0;
		}
		if (!SpecificWeapon)
		{
			if (abilityWeapon != itemEntityWeapon)
			{
				return 0;
			}
			return 1;
		}
		if (!Weapons.Contains(itemEntityWeapon.Blueprint))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "Ability Weapon Family";
	}
}
