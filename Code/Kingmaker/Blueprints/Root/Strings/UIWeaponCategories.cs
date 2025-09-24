using System;
using Kingmaker.Enums;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIWeaponCategories
{
	public LocalizedString WeaponCategoryMelee;

	public LocalizedString WeaponCategoryThrown;

	public LocalizedString WeaponCategoryPistol;

	public LocalizedString WeaponCategoryBasic;

	public LocalizedString WeaponCategoryHeavy;

	public LocalizedString WeaponFamilyLaser;

	public LocalizedString WeaponFamilySolid;

	public LocalizedString WeaponFamilyBolt;

	public LocalizedString WeaponFamilyMelta;

	public LocalizedString WeaponFamilyPlasma;

	public LocalizedString WeaponFamilyFlame;

	public LocalizedString WeaponFamilyExotic;

	public LocalizedString WeaponFamilyChain;

	public LocalizedString WeaponFamilyPower;

	public LocalizedString WeaponFamilyPrimitive;

	public LocalizedString WeaponFamilyForce;

	public LocalizedString WeaponFamilyBlade;

	public LocalizedString WeaponFamilyChainSaw;

	public LocalizedString WeaponClassificationSword;

	public LocalizedString WeaponClassificationAxe;

	public LocalizedString WeaponClassificationStaff;

	public LocalizedString WeaponClassificationChainsaw;

	public LocalizedString WeaponClassificationKnife;

	public LocalizedString WeaponClassificationMaul;

	public LocalizedString WeaponClassificationHammer;

	public LocalizedString WeaponClassificationWebber;

	public LocalizedString WeaponClassificationArcRifle;

	public LocalizedString WeaponClassificationSniperRifle;

	public LocalizedString WeaponHoldingTypeOneHanded;

	public LocalizedString WeaponHoldingTypeTwoHanded;

	public LocalizedString WeaponRangeRanged;

	public LocalizedString WeaponRangeMelee;

	public LocalizedString WeaponHeavinessHeavy;

	public LocalizedString WeaponHeavinessNotHeavy;

	public LocalizedString WeaponSubCategoryRanged;

	public LocalizedString WeaponSubCategoryMelee;

	public LocalizedString WeaponSubCategoryFinessable;

	public LocalizedString WeaponSubCategoryThrown;

	public LocalizedString WeaponSubCategoryNatural;

	public LocalizedString WeaponSubCategoryKnives;

	public LocalizedString WeaponSubCategoryMonk;

	public LocalizedString WeaponSubCategoryTwoHanded;

	public LocalizedString WeaponSubCategoryLight;

	public LocalizedString WeaponSubCategorySimple;

	public LocalizedString WeaponSubCategoryMartial;

	public LocalizedString WeaponSubCategoryExotic;

	public LocalizedString WeaponSubCategoryOneHandedPiercing;

	public LocalizedString WeaponSubCategoryDisabled;

	public LocalizedString WeaponSubCategoryOneHandedSlashing;

	public LocalizedString WeaponSubCategoryMetal;

	public string GetWeaponCategoryLabel(WeaponCategory category)
	{
		return category switch
		{
			WeaponCategory.Melee => WeaponCategoryMelee, 
			WeaponCategory.Thrown => WeaponCategoryThrown, 
			WeaponCategory.Pistol => WeaponCategoryPistol, 
			WeaponCategory.Basic => WeaponCategoryBasic, 
			WeaponCategory.Heavy => WeaponCategoryHeavy, 
			_ => string.Empty, 
		};
	}

	public string GetWeaponFamilyLabel(WeaponFamily family)
	{
		return family switch
		{
			WeaponFamily.Laser => WeaponFamilyLaser, 
			WeaponFamily.Solid => WeaponFamilySolid, 
			WeaponFamily.Bolt => WeaponFamilyBolt, 
			WeaponFamily.Melta => WeaponFamilyMelta, 
			WeaponFamily.Plasma => WeaponFamilyPlasma, 
			WeaponFamily.Flame => WeaponFamilyFlame, 
			WeaponFamily.Exotic => WeaponFamilyExotic, 
			WeaponFamily.Chain => WeaponFamilyChain, 
			WeaponFamily.Power => WeaponFamilyPower, 
			WeaponFamily.Primitive => WeaponFamilyPrimitive, 
			WeaponFamily.Force => WeaponFamilyForce, 
			WeaponFamily.Blade => WeaponFamilyBlade, 
			WeaponFamily.ChainSaw => WeaponFamilyChainSaw, 
			_ => string.Empty, 
		};
	}

	public string GetWeaponClassificationLabel(WeaponClassification classification)
	{
		switch (classification)
		{
		case WeaponClassification.Sword:
			return WeaponClassificationSword;
		case WeaponClassification.Axe:
			return WeaponClassificationAxe;
		case WeaponClassification.PsykerStaff:
		case WeaponClassification.NavigatorStaff:
			return WeaponClassificationStaff;
		case WeaponClassification.Chainsaw:
			return string.Empty;
		case WeaponClassification.Knife:
			return WeaponClassificationKnife;
		case WeaponClassification.Maul:
			return WeaponClassificationMaul;
		case WeaponClassification.MaulOrHammer:
		case WeaponClassification.Hammer:
			return WeaponClassificationHammer;
		case WeaponClassification.Webber:
			return WeaponClassificationWebber;
		case WeaponClassification.ArcRifle:
			return WeaponClassificationArcRifle;
		case WeaponClassification.SniperRifle:
			return WeaponClassificationSniperRifle;
		default:
			return string.Empty;
		}
	}

	public string GetWeaponHoldingTypeLabel(WeaponHoldingType type)
	{
		return type switch
		{
			WeaponHoldingType.OneHanded => WeaponHoldingTypeOneHanded, 
			WeaponHoldingType.TwoHanded => WeaponHoldingTypeTwoHanded, 
			_ => string.Empty, 
		};
	}

	public string GetWeaponRangeLabel(WeaponRange range)
	{
		return range switch
		{
			WeaponRange.Ranged => WeaponRangeRanged, 
			WeaponRange.Melee => WeaponRangeMelee, 
			_ => string.Empty, 
		};
	}

	public string GetWeaponHeavinessLabel(WeaponHeaviness heaviness)
	{
		return heaviness switch
		{
			WeaponHeaviness.Heavy => WeaponHeavinessHeavy, 
			WeaponHeaviness.NotHeavy => WeaponHeavinessNotHeavy, 
			_ => WeaponHeavinessNotHeavy, 
		};
	}

	public string GetWeaponSubCategoryLabel(WeaponSubCategory category)
	{
		return category switch
		{
			WeaponSubCategory.Ranged => WeaponSubCategoryRanged, 
			WeaponSubCategory.Melee => WeaponSubCategoryMelee, 
			WeaponSubCategory.Finessable => WeaponSubCategoryFinessable, 
			WeaponSubCategory.Thrown => WeaponSubCategoryThrown, 
			WeaponSubCategory.Natural => WeaponSubCategoryNatural, 
			WeaponSubCategory.Knives => WeaponSubCategoryKnives, 
			WeaponSubCategory.Monk => WeaponSubCategoryMonk, 
			WeaponSubCategory.TwoHanded => WeaponSubCategoryTwoHanded, 
			WeaponSubCategory.Light => WeaponSubCategoryLight, 
			WeaponSubCategory.Simple => WeaponSubCategorySimple, 
			WeaponSubCategory.Martial => WeaponSubCategoryMartial, 
			WeaponSubCategory.Exotic => WeaponSubCategoryExotic, 
			WeaponSubCategory.OneHandedPiercing => WeaponSubCategoryOneHandedPiercing, 
			WeaponSubCategory.Disabled => WeaponSubCategoryDisabled, 
			WeaponSubCategory.OneHandedSlashing => WeaponSubCategoryOneHandedSlashing, 
			WeaponSubCategory.Metal => WeaponSubCategoryMetal, 
			_ => string.Empty, 
		};
	}
}
