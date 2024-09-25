using Kingmaker.Settings.Difficulty;

namespace Kingmaker.Settings;

public static class DifficultySettingsExtensions
{
	public static DifficultyPreset ToDifficultyPreset(this DifficultySettings settings)
	{
		return new DifficultyPreset
		{
			GameDifficulty = settings.GameDifficulty,
			RespecAllowed = settings.RespecAllowed,
			AutoLevelUp = settings.AutoLevelUp,
			AdditionalAIBehaviors = settings.AdditionalAIBehaviors,
			CombatEncountersCapacity = settings.CombatEncountersCapacity,
			EnemyDodgePercentModifier = settings.EnemyDodgePercentModifier,
			CoverHitBonusHalfModifier = settings.CoverHitBonusHalfModifier,
			CoverHitBonusFullModifier = settings.CoverHitBonusFullModifier,
			MinPartyDamage = settings.MinPartyDamage,
			MinPartyDamageFraction = settings.MinPartyDamageFraction,
			MinPartyStarshipDamage = settings.MinPartyStarshipDamage,
			MinPartyStarshipDamageFraction = settings.MinPartyStarshipDamageFraction,
			PartyMomentumPercentModifier = settings.PartyMomentumPercentModifier,
			NPCAttributesBaseValuePercentModifier = settings.NPCAttributesBaseValuePercentModifier,
			HardCrowdControlOnPartyMaxDurationRounds = settings.HardCrowdControlOnPartyMaxDurationRounds,
			SkillCheckModifier = settings.SkillCheckModifier,
			EnemyHitPointsPercentModifier = settings.EnemyHitPointsPercentModifier,
			AllyResolveModifier = settings.AllyResolveModifier,
			PartyDamageDealtAfterArmorReductionPercentModifier = settings.PartyDamageDealtAfterArmorReductionPercentModifier,
			WoundDamagePerTurnThresholdHPFraction = settings.WoundDamagePerTurnThresholdHPFraction,
			OldWoundDelayRounds = settings.OldWoundDelayRounds,
			WoundStacksForTrauma = settings.WoundStacksForTrauma,
			SpaceCombatDifficulty = settings.SpaceCombatDifficulty
		};
	}

	public static DifficultyPreset TempToDifficultyPreset(this DifficultySettings settings)
	{
		return new DifficultyPreset
		{
			GameDifficulty = settings.GameDifficulty.GetTempValue(),
			RespecAllowed = settings.RespecAllowed.GetTempValue(),
			AutoLevelUp = settings.AutoLevelUp.GetTempValue(),
			AdditionalAIBehaviors = settings.AdditionalAIBehaviors.GetTempValue(),
			CombatEncountersCapacity = settings.CombatEncountersCapacity.GetTempValue(),
			EnemyDodgePercentModifier = settings.EnemyDodgePercentModifier.GetTempValue(),
			CoverHitBonusHalfModifier = settings.CoverHitBonusHalfModifier.GetTempValue(),
			CoverHitBonusFullModifier = settings.CoverHitBonusFullModifier.GetTempValue(),
			MinPartyDamage = settings.MinPartyDamage.GetTempValue(),
			MinPartyDamageFraction = settings.MinPartyDamageFraction.GetTempValue(),
			MinPartyStarshipDamage = settings.MinPartyStarshipDamage.GetTempValue(),
			MinPartyStarshipDamageFraction = settings.MinPartyStarshipDamageFraction.GetTempValue(),
			PartyMomentumPercentModifier = settings.PartyMomentumPercentModifier.GetTempValue(),
			NPCAttributesBaseValuePercentModifier = settings.NPCAttributesBaseValuePercentModifier.GetTempValue(),
			HardCrowdControlOnPartyMaxDurationRounds = settings.HardCrowdControlOnPartyMaxDurationRounds.GetTempValue(),
			SkillCheckModifier = settings.SkillCheckModifier.GetTempValue(),
			EnemyHitPointsPercentModifier = settings.EnemyHitPointsPercentModifier.GetTempValue(),
			AllyResolveModifier = settings.AllyResolveModifier.GetTempValue(),
			PartyDamageDealtAfterArmorReductionPercentModifier = settings.PartyDamageDealtAfterArmorReductionPercentModifier.GetTempValue(),
			WoundDamagePerTurnThresholdHPFraction = settings.WoundDamagePerTurnThresholdHPFraction.GetTempValue(),
			OldWoundDelayRounds = settings.OldWoundDelayRounds.GetTempValue(),
			WoundStacksForTrauma = settings.WoundStacksForTrauma.GetTempValue(),
			SpaceCombatDifficulty = settings.SpaceCombatDifficulty.GetTempValue()
		};
	}
}
