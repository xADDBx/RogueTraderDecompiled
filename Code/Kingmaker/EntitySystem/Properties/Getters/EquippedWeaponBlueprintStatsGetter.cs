using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("82291f7a1d9b4de39ab337e8712715d4")]
public class EquippedWeaponBlueprintStatsGetter : MechanicEntityPropertyGetter, PropertyContextAccessor.IAbilityWeapon, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private CurrentWeaponBlueprintStatsGetter.WeaponStatType StatType;

	public bool RestrictByFamily;

	[ShowIf("RestrictByFamily")]
	public WeaponFamily AllowedWeaponFamily;

	[ShowIf("RestrictByFamily")]
	public bool InvertFamilyRestriction;

	public bool RestrictByCategory;

	[ShowIf("RestrictByCategory")]
	public WeaponCategory AllowedWeaponCategory;

	[ShowIf("RestrictByCategory")]
	public bool InvertCategoryRestriction;

	public bool RestrictByEquipmentFact;

	[ShowIf("RestrictByEquipmentFact")]
	private List<BlueprintUnitFactReference> AllowedFacts;

	[ShowIf("RestrictByEquipmentFact")]
	public bool InvertFactRestriction;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return StatType.ToString() + " of equipped weapon under restrictions";
	}

	protected override int GetBaseValue()
	{
		ItemEntityWeapon itemEntityWeapon = null;
		IEnumerable<ItemEntityWeapon> enumerable = (from h in base.CurrentEntity.GetBodyOptional()?.CurrentHandsEquipmentSet.Hands
			select h.MaybeWeapon into w
			where (!RestrictByFamily || (InvertFamilyRestriction ? (w.Blueprint.Family != AllowedWeaponFamily) : (w.Blueprint.Family == AllowedWeaponFamily))) && (!RestrictByCategory || ((!InvertCategoryRestriction) ? (w.Blueprint.Category == AllowedWeaponCategory) : (w.Blueprint.Category != AllowedWeaponCategory)))
			select w);
		if (enumerable != null && !enumerable.Empty())
		{
			if (!RestrictByEquipmentFact)
			{
				itemEntityWeapon = enumerable.FirstOrDefault();
			}
			else
			{
				foreach (ItemEntityWeapon item in enumerable)
				{
					BlueprintComponentsEnumerator<AddFactToEquipmentWielder> facts = item.Blueprint.GetComponents<AddFactToEquipmentWielder>();
					if (InvertFactRestriction)
					{
						if (AllowedFacts.All((BlueprintUnitFactReference allowedFact) => facts.All((AddFactToEquipmentWielder f) => allowedFact.Get() != f.Fact)))
						{
							itemEntityWeapon = item;
							break;
						}
					}
					else if (AllowedFacts.Any((BlueprintUnitFactReference allowedFact) => facts.Any((AddFactToEquipmentWielder f) => allowedFact.Get() == f.Fact)))
					{
						itemEntityWeapon = item;
						break;
					}
				}
			}
		}
		if (itemEntityWeapon == null)
		{
			PFLog.EntityFact.Warning($"no valid weapon for EquippedWeaponBlueprintStatsGetter of {base.CurrentEntity}");
		}
		switch (StatType)
		{
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.Damage:
			return itemEntityWeapon.Blueprint.WarhammerDamage;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.MaxDamage:
			return itemEntityWeapon.Blueprint.WarhammerMaxDamage;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.Penetration:
			return itemEntityWeapon.Blueprint.WarhammerPenetration;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.DodgePenetration:
			return itemEntityWeapon.Blueprint.DodgePenetration;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.AdditionalHitChance:
			return itemEntityWeapon.Blueprint.AdditionalHitChance;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.Recoil:
			return itemEntityWeapon.Blueprint.WarhammerRecoil;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.MaxDistance:
			return itemEntityWeapon.Blueprint.WarhammerMaxDistance;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.MaxAmmo:
			return itemEntityWeapon.Blueprint.WarhammerMaxAmmo;
		case CurrentWeaponBlueprintStatsGetter.WeaponStatType.RateOfFire:
			return itemEntityWeapon.Blueprint.RateOfFire;
		default:
			PFLog.Ability.Error($"Wrong stat type in EquippedWeaponBlueprintStatsGetter : {StatType}");
			return 0;
		}
	}
}
