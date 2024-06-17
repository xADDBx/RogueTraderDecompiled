using System;
using System.Linq;
using Kingmaker.Enums;
using Kingmaker.Settings.Difficulty;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class DifficultySettings
{
	public readonly SettingsEntityEnum<GameDifficultyOption> GameDifficulty;

	public readonly SettingsEntityEnum<CombatEncountersCapacity> CombatEncountersCapacity;

	public readonly SettingsEntityEnum<AutoLevelUpOption> AutoLevelUp;

	public readonly SettingsEntityBool RespecAllowed;

	public readonly SettingsEntityBool AdditionalAIBehaviors;

	public readonly SettingsEntityBool OnlyOneSave;

	public readonly SettingsEntityBool LimitedAI;

	public readonly SettingsEntityInt EnemyDodgePercentModifier;

	public readonly SettingsEntityInt CoverHitBonusHalfModifier;

	public readonly SettingsEntityInt CoverHitBonusFullModifier;

	public readonly SettingsEntityInt MinPartyDamage;

	public readonly SettingsEntityInt MinPartyDamageFraction;

	public readonly SettingsEntityInt MinPartyStarshipDamage;

	public readonly SettingsEntityInt MinPartyStarshipDamageFraction;

	public readonly SettingsEntityInt PartyMomentumPercentModifier;

	public readonly SettingsEntityInt NPCAttributesBaseValuePercentModifier;

	public readonly SettingsEntityEnum<HardCrowdControlDurationLimit> HardCrowdControlOnPartyMaxDurationRounds;

	public readonly SettingsEntityInt SkillCheckModifier;

	public readonly SettingsEntityInt EnemyHitPointsPercentModifier;

	public readonly SettingsEntityInt AllyResolveModifier;

	public readonly SettingsEntityInt PartyDamageDealtAfterArmorReductionPercentModifier;

	public readonly SettingsEntityInt WoundDamagePerTurnThresholdHPFraction;

	public readonly SettingsEntityInt OldWoundDelayRounds;

	public readonly SettingsEntityInt WoundStacksForTrauma;

	public readonly SettingsEntityInt MinCRScaling;

	public readonly SettingsEntityInt MaxCRScaling;

	private const int MinCRScalingFixed = 0;

	private const int MaxCRScalingFixed = 15;

	public readonly SettingsEntityEnum<SpaceCombatDifficulty> SpaceCombatDifficulty;

	public DifficultySettings(ISettingsController settingsController, SettingsValues settingsValues)
	{
		DifficultySettingsDefaultValues defaultValues = settingsValues.SettingsDefaultValues.Difficulty;
		DifficultyPreset difficultyPreset = settingsValues.DifficultiesPresets.Difficulties.FirstOrDefault((DifficultyPresetAsset p) => p.Preset.GameDifficulty == defaultValues.GameDifficulty)?.Preset;
		if (difficultyPreset == null)
		{
			throw new Exception($"DifficultySettings: couldn't find {defaultValues.GameDifficulty} preset in current SettingsDefaultValues");
		}
		GameDifficulty = new SettingsEntityEnum<GameDifficultyOption>(settingsController, "game-difficulty", difficultyPreset.GameDifficulty, saveDependent: true);
		CombatEncountersCapacity = new SettingsEntityEnum<CombatEncountersCapacity>(settingsController, "combat-encounters-capacity", difficultyPreset.CombatEncountersCapacity, saveDependent: true);
		AutoLevelUp = new SettingsEntityEnum<AutoLevelUpOption>(settingsController, "auto-level-up", difficultyPreset.AutoLevelUp, saveDependent: true);
		RespecAllowed = new SettingsEntityBool(settingsController, "respec-allowed", difficultyPreset.RespecAllowed, saveDependent: true);
		AdditionalAIBehaviors = new SettingsEntityBool(settingsController, "additional-ai-behaviours", difficultyPreset.AdditionalAIBehaviors, saveDependent: true);
		OnlyOneSave = new SettingsEntityBool(settingsController, "only-one-save", defaultValues.OnlyOneSave, saveDependent: true);
		LimitedAI = new SettingsEntityBool(settingsController, "limited-ai", defaultValues.LimitedAI, saveDependent: true);
		EnemyDodgePercentModifier = new SettingsEntityInt(settingsController, "enemy-dodge-percent-modifier", difficultyPreset.EnemyDodgePercentModifier, saveDependent: true);
		CoverHitBonusHalfModifier = new SettingsEntityInt(settingsController, "cover-hit-bonus-half-modifier", difficultyPreset.CoverHitBonusHalfModifier, saveDependent: true);
		CoverHitBonusFullModifier = new SettingsEntityInt(settingsController, "cover-hit-bonus-full-modifier", difficultyPreset.CoverHitBonusFullModifier, saveDependent: true);
		MinPartyDamage = new SettingsEntityInt(settingsController, "min-party-damage", difficultyPreset.MinPartyDamage, saveDependent: true);
		MinPartyDamageFraction = new SettingsEntityInt(settingsController, "min-party-damage-fraction", difficultyPreset.MinPartyDamageFraction, saveDependent: true);
		MinPartyStarshipDamage = new SettingsEntityInt(settingsController, "min-party-starship-damage", difficultyPreset.MinPartyStarshipDamage, saveDependent: true);
		MinPartyStarshipDamageFraction = new SettingsEntityInt(settingsController, "min-party-starship-damage-fraction", difficultyPreset.MinPartyStarshipDamageFraction, saveDependent: true);
		PartyMomentumPercentModifier = new SettingsEntityInt(settingsController, "party-momentum-percent-modifier", difficultyPreset.PartyMomentumPercentModifier, saveDependent: true);
		NPCAttributesBaseValuePercentModifier = new SettingsEntityInt(settingsController, "npc-attributes-base-value-percent-modifier", difficultyPreset.NPCAttributesBaseValuePercentModifier, saveDependent: true);
		HardCrowdControlOnPartyMaxDurationRounds = new SettingsEntityEnum<HardCrowdControlDurationLimit>(settingsController, "hard-crowd-control-on-party-max-duration-rounds", difficultyPreset.HardCrowdControlOnPartyMaxDurationRounds, saveDependent: true);
		SkillCheckModifier = new SettingsEntityInt(settingsController, "skill-check-modifier", difficultyPreset.SkillCheckModifier, saveDependent: true);
		EnemyHitPointsPercentModifier = new SettingsEntityInt(settingsController, "enemy-hit-points-percent-modifier", difficultyPreset.EnemyHitPointsPercentModifier, saveDependent: true);
		AllyResolveModifier = new SettingsEntityInt(settingsController, "party-resolve-modifier", difficultyPreset.AllyResolveModifier, saveDependent: true);
		PartyDamageDealtAfterArmorReductionPercentModifier = new SettingsEntityInt(settingsController, "party-damage-dealt-after-armor-reduction-percent-modifier", difficultyPreset.PartyDamageDealtAfterArmorReductionPercentModifier, saveDependent: true);
		WoundDamagePerTurnThresholdHPFraction = new SettingsEntityInt(settingsController, "wound-damage-per-turn-threshold-hp-fraction", difficultyPreset.WoundDamagePerTurnThresholdHPFraction, saveDependent: true);
		OldWoundDelayRounds = new SettingsEntityInt(settingsController, "old-wound-delay-rounds", difficultyPreset.OldWoundDelayRounds, saveDependent: true);
		WoundStacksForTrauma = new SettingsEntityInt(settingsController, "wound-stacks-for-trauma", difficultyPreset.WoundStacksForTrauma, saveDependent: true);
		MinCRScaling = new SettingsEntityInt(settingsController, "min-cr-for-scaling", 0, saveDependent: true);
		MaxCRScaling = new SettingsEntityInt(settingsController, "max-cr-for-scaling", 15, saveDependent: true);
		SpaceCombatDifficulty = new SettingsEntityEnum<SpaceCombatDifficulty>(settingsController, "space-combat-difficulty", difficultyPreset.SpaceCombatDifficulty, saveDependent: true);
	}
}
