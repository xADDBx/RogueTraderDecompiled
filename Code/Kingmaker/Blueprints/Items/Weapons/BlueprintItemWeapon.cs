using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.RuleSystem.Enum;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Weapons;

[TypeId("c00f723cccf2d314198c42a572c631fd")]
public class BlueprintItemWeapon : BlueprintItemEquipmentHand
{
	[SerializeField]
	private BlueprintAbilityAdditionalEffect.Reference m_OnHitActions;

	[SerializeField]
	[JsonProperty(PropertyName = "AbilityContainer")]
	public WeaponAbilityContainer WeaponAbilities;

	[SerializeField]
	[ShowIf("IsMelee")]
	private BlueprintAbilityReference m_AttackOfOpportunityAbility;

	[SerializeField]
	[ShowIf("IsMelee")]
	private BlueprintAbilityFXSettings.Reference m_AttackOfOpportunityAbilityFXSettings;

	public WeaponCategory Category;

	public WeaponFamily Family;

	public WeaponClassification Classification;

	[Space]
	[SerializeField]
	private WeaponHoldingType m_HoldingType;

	[SerializeField]
	private WeaponRange m_Range;

	[SerializeField]
	private WeaponHeaviness m_Heaviness;

	[Space]
	public bool CanBeUsedInGame;

	[Space]
	[SerializeField]
	private LocalizedString m_TypeNameText;

	[SerializeField]
	private LocalizedString m_DefaultNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	public bool IsTwoHanded;

	public int WarhammerDamage;

	public int WarhammerMaxDamage;

	public int WarhammerPenetration;

	public int DodgePenetration;

	public int AdditionalHitChance;

	public int WarhammerRecoil;

	public int WarhammerMaxDistance;

	public int WarhammerMaxAmmo = -1;

	public int RateOfFire = 1;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintWeaponEnchantmentReference[] m_Enchantments;

	[SerializeField]
	private DamageTypeDescription m_DamageType;

	[SerializeField]
	private bool m_IsNatural;

	[SerializeField]
	private DamageStatBonusFactor m_DamageStatBonusFactor;

	public override IEnumerable<BlueprintAbility> Abilities => base.Abilities.Concat(WeaponAbilities.Select((WeaponAbility i) => i.Ability).NotNull());

	public override bool GainAbility
	{
		get
		{
			if (!base.GainAbility)
			{
				return WeaponAbilities.Any();
			}
			return true;
		}
	}

	[CanBeNull]
	public BlueprintAbilityAdditionalEffect OnHitActions => m_OnHitActions;

	public override string SubtypeName => m_TypeNameText;

	public override string SubtypeDescription => m_DescriptionText;

	public BlueprintAbility AttackOfOpportunityAbility
	{
		get
		{
			if (!IsMelee || m_AttackOfOpportunityAbility == null)
			{
				return null;
			}
			return m_AttackOfOpportunityAbility.Get();
		}
	}

	public BlueprintAbilityFXSettings AttackOfOpportunityAbilityFXSettings
	{
		get
		{
			if (!IsMelee || m_AttackOfOpportunityAbilityFXSettings == null)
			{
				return null;
			}
			return m_AttackOfOpportunityAbilityFXSettings.Get();
		}
	}

	public int? OverrideOverpenetrationFactorPercents => this.GetComponent<OverrideOverpenetrationFactor>()?.FactorPercents;

	public override string Name
	{
		get
		{
			if (!string.IsNullOrEmpty(base.Name))
			{
				return base.Name;
			}
			string text = m_DefaultNameText;
			string text2 = GetEnchantmentPrefixes() + text + GetEnchantmentSuffixes();
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
			if (!string.IsNullOrEmpty(m_TypeNameText))
			{
				return m_TypeNameText;
			}
			string text3 = LocalizedTexts.Instance.Stats.GetText(Category);
			if (string.IsNullOrEmpty(text3))
			{
				return UIStrings.Instance.CharacterSheet.Attack;
			}
			return text3;
		}
	}

	public override string Description
	{
		get
		{
			string description = base.Description;
			if (!string.IsNullOrWhiteSpace(description))
			{
				return description;
			}
			return m_DescriptionText;
		}
	}

	public override ItemsItemType ItemType => ItemsItemType.Weapon;

	public AttackType AttackType
	{
		get
		{
			if (!IsMelee)
			{
				return AttackType.Ranged;
			}
			return AttackType.Melee;
		}
	}

	public int AttackRange => WarhammerMaxDistance;

	public int AttackOptimalRange
	{
		get
		{
			if (!IsMelee)
			{
				return WarhammerMaxDistance / 2;
			}
			return WarhammerMaxDistance;
		}
	}

	public DamageTypeDescription DamageType => m_DamageType;

	public bool IsNatural => m_IsNatural;

	public bool IsRanged => !IsMelee;

	public bool IsMelee => Category == WeaponCategory.Melee;

	public override float Weight => m_Weight;

	public WeaponHoldingType HoldingType => m_HoldingType;

	public WeaponRange Range => m_Range;

	public WeaponHeaviness Heaviness => m_Heaviness;

	public bool HasNoDamage => WarhammerMaxDamage == 0;

	public DamageStatBonusFactor DamageStatBonusFactor => m_DamageStatBonusFactor;

	public override void Reset()
	{
		base.Reset();
		SpendCharges = false;
	}

	public string GetEnchantmentSuffixes()
	{
		if (m_Enchantments == null || m_Enchantments.Empty())
		{
			return "";
		}
		string text = "";
		foreach (BlueprintWeaponEnchantment item in m_Enchantments.Dereference())
		{
			if (!item.Suffix.Empty())
			{
				text = text + " " + item.Suffix;
			}
		}
		return text;
	}

	public string GetEnchantmentPrefixes()
	{
		if (m_Enchantments == null || m_Enchantments.Empty())
		{
			return "";
		}
		string text = "";
		foreach (BlueprintWeaponEnchantment item in m_Enchantments.Dereference())
		{
			if (!item.Prefix.Empty())
			{
				text = text + item.Prefix + " ";
			}
		}
		return text;
	}

	protected override IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		return m_Enchantments.EmptyIfNull().Dereference();
	}
}
