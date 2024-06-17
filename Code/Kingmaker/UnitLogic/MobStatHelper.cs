using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.UnitLogic;

public static class MobStatHelper
{
	public static bool IsStatTypeSecondary(StatType statType, bool isMelee, BlueprintUnit.ArmyStat attributeSettings, bool isProfessional, bool isRanged, bool isBoss)
	{
		if (isMelee && ((statType == StatType.WarhammerWeaponSkill && !attributeSettings.NotModified && attributeSettings.Modifier < 0) || (statType == StatType.WarhammerStrength && !attributeSettings.NotModified && attributeSettings.Modifier < 0) || (statType == StatType.WarhammerAgility && !isProfessional && (attributeSettings.NotModified || attributeSettings.Modifier == 0))))
		{
			return true;
		}
		if (isRanged && ((statType == StatType.WarhammerBallisticSkill && !attributeSettings.NotModified && attributeSettings.Modifier < 0) || (statType == StatType.WarhammerPerception && ((isProfessional && attributeSettings.NotModified) || (!attributeSettings.NotModified && attributeSettings.Modifier > 0)))))
		{
			return true;
		}
		if (isBoss && statType == StatType.WarhammerWillpower && !isProfessional && attributeSettings.NotModified)
		{
			return true;
		}
		if (!isProfessional || (!attributeSettings.NotModified && attributeSettings.Modifier != 0))
		{
			if (!isProfessional && !attributeSettings.NotModified)
			{
				return attributeSettings.Modifier > 0;
			}
			return false;
		}
		return true;
	}

	public static bool IsStatTypePrimary(StatType statType, bool isMelee, bool isProfessional, BlueprintUnit.ArmyStat attributeSettings, bool isRanged, bool isBoss, MobTypeForStatCalculations? newType)
	{
		if (isMelee && (statType == StatType.WarhammerWeaponSkill || statType == StatType.WarhammerStrength || (statType == StatType.WarhammerAgility && isProfessional && !attributeSettings.NotModified && attributeSettings.Modifier > 0)))
		{
			return true;
		}
		if (isRanged && (statType == StatType.WarhammerBallisticSkill || (statType == StatType.WarhammerPerception && isProfessional && !attributeSettings.NotModified && attributeSettings.Modifier > 0)))
		{
			return true;
		}
		if (isBoss && statType == StatType.WarhammerWillpower && isProfessional && !attributeSettings.NotModified && attributeSettings.Modifier > 0)
		{
			return true;
		}
		if (ReplaceMobTypeForStatCalculations.IsMobTypeForStatCalculationsLeader(newType) && statType == StatType.WarhammerFellowship)
		{
			return true;
		}
		if (ReplaceMobTypeForStatCalculations.IsMobTypeForStatCalculationsPsyker(newType) && statType == StatType.WarhammerWillpower)
		{
			return true;
		}
		if (isProfessional && !attributeSettings.NotModified)
		{
			return attributeSettings.Modifier > 0;
		}
		return false;
	}

	public static bool IsMobTypeRanged(bool isMelee, MobTypeForStatCalculations? newType)
	{
		if (!isMelee || ReplaceMobTypeForStatCalculations.IsMobTypeForStatCalculationsRanged(newType))
		{
			return !ReplaceMobTypeForStatCalculations.IsMobTypeForStatCalculationsNotRanged(newType);
		}
		return false;
	}

	public static bool IsMobTypeMelee(BlueprintItemWeapon primaryHand, BlueprintItemWeapon secondaryHand, BlueprintItemWeapon primaryHandAlternative, BlueprintItemWeapon secondaryHandAlternative, MobTypeForStatCalculations? newType)
	{
		if ((primaryHand != null && primaryHand.IsMelee) || (secondaryHand != null && secondaryHand.IsMelee) || (primaryHandAlternative != null && primaryHandAlternative.IsMelee) || (secondaryHandAlternative != null && secondaryHandAlternative.IsMelee) || ReplaceMobTypeForStatCalculations.IsMobTypeForStatCalculationsMelee(newType))
		{
			return !ReplaceMobTypeForStatCalculations.IsMobTypeForStatCalculationsNotMelee(newType);
		}
		return false;
	}
}
