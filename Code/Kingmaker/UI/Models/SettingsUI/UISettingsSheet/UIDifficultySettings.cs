using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.MainMenu;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIDifficultySettings : IUISettingsSheet
{
	public UISettingsEntityGameDifficulty GameDifficulty;

	public UISettingsEntityDropdownCombatEncountersCapacity CombatEncountersCapacity;

	public UISettingsEntityDropdownAutoLevelUp AutoLevelUp;

	public UISettingsEntityBool RespecAllowed;

	public UISettingsEntityBool AdditionalAIBehaviors;

	public UISettingsEntityBool OnlyOneSave;

	public UISettingsEntityBool LimitedAI;

	public UISettingsEntitySliderInt EnemyDodgePercentModifier;

	public UISettingsEntitySliderInt CoverHitBonusHalfModifier;

	public UISettingsEntitySliderInt CoverHitBonusFullModifier;

	public UISettingsEntitySliderInt MinPartyDamage;

	public UISettingsEntitySliderInt MinPartyDamageFraction;

	public UISettingsEntitySliderInt MinPartyStarshipDamage;

	public UISettingsEntitySliderInt MinPartyStarshipDamageFraction;

	public UISettingsEntitySliderInt PartyMomentumPercentMultiplier;

	public UISettingsEntitySliderInt NPCAttributesBaseValuePercentMultiplier;

	public UISettingDropdownHardCrowdControlDurationLimit HardCrowdControlOnPartyMaxDurationRounds;

	public UISettingsEntitySliderInt SkillCheckModifier;

	public UISettingsEntitySliderInt EnemyHitPointsPercentModifier;

	public UISettingsEntitySliderInt PartyDamageDealtAfterArmorReductionPercentModifier;

	public UISettingsEntitySliderInt WoundDamagePerTurnThresholdHPFraction;

	public UISettingsEntitySliderInt OldWoundDelayRounds;

	public UISettingsEntitySliderInt WoundStacksForTrauma;

	public UISettingsEntityDropdownSpaceCombatDifficulty SpaceCombatDifficulty;

	public void LinkToSettings()
	{
		GameDifficulty.LinkSetting(SettingsRoot.Difficulty.GameDifficulty);
		CombatEncountersCapacity.LinkSetting(SettingsRoot.Difficulty.CombatEncountersCapacity);
		AutoLevelUp.LinkSetting(SettingsRoot.Difficulty.AutoLevelUp);
		RespecAllowed.LinkSetting(SettingsRoot.Difficulty.RespecAllowed);
		AdditionalAIBehaviors.LinkSetting(SettingsRoot.Difficulty.AdditionalAIBehaviors);
		OnlyOneSave.LinkSetting(SettingsRoot.Difficulty.OnlyOneSave);
		LimitedAI.LinkSetting(SettingsRoot.Difficulty.LimitedAI);
		EnemyDodgePercentModifier.LinkSetting(SettingsRoot.Difficulty.EnemyDodgePercentModifier);
		CoverHitBonusHalfModifier.LinkSetting(SettingsRoot.Difficulty.CoverHitBonusHalfModifier);
		CoverHitBonusFullModifier.LinkSetting(SettingsRoot.Difficulty.CoverHitBonusFullModifier);
		MinPartyDamage.LinkSetting(SettingsRoot.Difficulty.MinPartyDamage);
		MinPartyDamageFraction.LinkSetting(SettingsRoot.Difficulty.MinPartyDamageFraction);
		MinPartyStarshipDamage.LinkSetting(SettingsRoot.Difficulty.MinPartyStarshipDamage);
		MinPartyStarshipDamageFraction.LinkSetting(SettingsRoot.Difficulty.MinPartyStarshipDamageFraction);
		PartyMomentumPercentMultiplier.LinkSetting(SettingsRoot.Difficulty.PartyMomentumPercentModifier);
		NPCAttributesBaseValuePercentMultiplier.LinkSetting(SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier);
		HardCrowdControlOnPartyMaxDurationRounds.LinkSetting(SettingsRoot.Difficulty.HardCrowdControlOnPartyMaxDurationRounds);
		SkillCheckModifier.LinkSetting(SettingsRoot.Difficulty.SkillCheckModifier);
		EnemyHitPointsPercentModifier.LinkSetting(SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
		PartyDamageDealtAfterArmorReductionPercentModifier.LinkSetting(SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier);
		WoundDamagePerTurnThresholdHPFraction.LinkSetting(SettingsRoot.Difficulty.WoundDamagePerTurnThresholdHPFraction);
		OldWoundDelayRounds.LinkSetting(SettingsRoot.Difficulty.OldWoundDelayRounds);
		WoundStacksForTrauma.LinkSetting(SettingsRoot.Difficulty.WoundStacksForTrauma);
		SpaceCombatDifficulty.LinkSetting(SettingsRoot.Difficulty.SpaceCombatDifficulty);
	}

	public void InitializeSettings()
	{
		OnlyOneSave.ModificationAllowedCheck = () => MainMenuUI.IsActive;
		OnlyOneSave.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.OnlyOneSave);
	}
}
