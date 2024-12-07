using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UITooltips
{
	public LocalizedString BaseDamage;

	public LocalizedString CannotbeEquip;

	public LocalizedString CanBeEquip;

	public LocalizedString CanBeUsed;

	public LocalizedString CannotbeUsed;

	public LocalizedString IsNotRemovable;

	public LocalizedString Loot;

	public LocalizedString Door;

	public LocalizedString DoorOpen;

	public LocalizedString DoorClose;

	public LocalizedString Trap;

	public LocalizedString TrapNeutralize;

	public LocalizedString Ladder;

	public LocalizedString TitlePreviewSkillcheckSkillDC;

	public LocalizedString TipPreviewSkillcheckBestCharacter;

	public LocalizedString NoFeature;

	public LocalizedString CharacterLevel;

	public LocalizedString Level;

	public LocalizedString NoClass;

	public LocalizedString OneFrom;

	public LocalizedString FeaturesFrom;

	public LocalizedString NoProficiencies;

	public LocalizedString HasProficiencies;

	public LocalizedString MoreThan;

	public LocalizedString and;

	public LocalizedString or;

	public LocalizedString lbs;

	public LocalizedString PartyEncumbrance;

	public LocalizedString PersonalEncumbrance;

	public LocalizedString[] EncumbranceStatus = new LocalizedString[4];

	public LocalizedString CurrentValue;

	public LocalizedString Ranks;

	public LocalizedString BaseValue;

	public LocalizedString TotalValue;

	public LocalizedString TotalSkillValue;

	public LocalizedString TotalAttributeValue;

	public LocalizedString BonusValue;

	public LocalizedString UnitIsNotInspected;

	public LocalizedString CurrentLevelExperience;

	public LocalizedString NextLevelExperience;

	public LocalizedString TillNextLevelExperience;

	public LocalizedString TwoHanded;

	public LocalizedString OneHanded;

	public LocalizedString NoItemsAvailableToSelect;

	public LocalizedString NonStackHeaderLabel;

	public LocalizedString ShowInfo;

	public LocalizedString Source;

	public LocalizedString RateOfFire;

	public LocalizedString Recoil;

	public LocalizedString MaximumRange;

	public LocalizedString CostAP;

	public LocalizedString AP;

	public LocalizedString MP;

	public LocalizedString PsychicPowerCostAP;

	public LocalizedString ReloadAP;

	public LocalizedString PsykerPower;

	public LocalizedString EndsTurn;

	public LocalizedString SpendAllMovementPoints;

	public LocalizedString AttackAbilityGroupCooldown;

	public LocalizedString IncreaseVeilDegradation;

	public LocalizedString MinorVeilDegradation;

	public LocalizedString MajorVeilDegradation;

	public LocalizedString MeleeStrikesCount;

	public LocalizedString ShotsCount;

	public LocalizedString MomentumAvailable;

	public LocalizedString MomentumNotAvailable;

	public LocalizedString EndTurn;

	public LocalizedString HeroicActAbility;

	public LocalizedString DesperateMeasureAbility;

	public LocalizedString HitChances;

	public LocalizedString HitChancesEffectiveDistance;

	public LocalizedString HitChancesMaxDistance;

	public LocalizedString ItemFooterLabel;

	public LocalizedString SpendAllMovementPointsShort;

	public LocalizedString AttackAbilityGroupCooldownShort;

	public LocalizedString IncreaseVeilDegradationShort;

	public LocalizedString ArmourDamageReduceDescription;

	public LocalizedString ArmourDodgeChanceDescription;

	public LocalizedString ReplenishingItem;

	public LocalizedString ScatterMainLineClose;

	public LocalizedString ScatterClose;

	public LocalizedString ScatterMainLine;

	public LocalizedString ScatterNear;

	public LocalizedString ScatterFar;

	public LocalizedString BonusesSum;

	public LocalizedString BaseChance;

	public LocalizedString DifficultyReduceDescription;

	public LocalizedString HPLeft;

	public LocalizedString HPMax;

	public LocalizedString HPTemporary;

	public LocalizedString HPTotalLeft;

	public LocalizedString HPTotalMax;

	public LocalizedString PossibleToKill;

	public LocalizedString Damage;

	public LocalizedString InitialDamage;

	public LocalizedString BurstCount;

	public LocalizedString PossibleToPush;

	public LocalizedString EveryRound;

	public LocalizedString TotalHitChance;

	public LocalizedString InitialHitChance;

	public LocalizedString DodgeAvoidance;

	public LocalizedString ParryAvoidance;

	public LocalizedString CoverAvoidance;

	public LocalizedString YouWillGainTitle;

	public LocalizedString YouWillLoseTitle;

	public LocalizedString ReputationPointsAbbreviation;

	public LocalizedString CommonFeatureDesc;

	public LocalizedString FlipZoneStrategist;

	public LocalizedString FlipZoneStrategistConsole;

	[Header("Hints")]
	public LocalizedString ShowTooltipHint;

	public LocalizedString HideTooltipHint;

	public LocalizedString ShowComparativeHint;

	public LocalizedString HideComparativeHint;

	[Header("SoulMarks")]
	public LocalizedString SoulMarkRankHeader;

	public LocalizedString SoulMarkRankDescription;

	public LocalizedString SoulMarkIsLocked;

	public LocalizedString SoulMarkMayBeLocked;

	public LocalizedString SoulMarkCompanion;

	[Header("Prerequisites")]
	public LocalizedString Prerequisites;

	public LocalizedString PrerequisiteAbilities;

	public LocalizedString PrerequisiteCareers;

	public LocalizedString PrerequisiteFeatures;

	public LocalizedString PrerequisiteRank;

	public LocalizedString PrerequisiteLevel;

	public LocalizedString PrerequisitesFooter;

	public LocalizedString ToCurrentPrerequisiteFeature;

	[Header("CharGen")]
	public LocalizedString DoctrinesHeader;

	public LocalizedString DoctrinesShortDesc;

	public LocalizedString DoctrinesDescription;
}
