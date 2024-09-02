using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

namespace Kingmaker.Settings.Difficulty;

public class DifficultySettingsController
{
	private readonly DifficultySettings m_Settings;

	private bool m_HasUpdatedSettings;

	public DifficultySettingsController()
	{
		m_Settings = SettingsRoot.Difficulty;
		m_Settings.GameDifficulty.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.RespecAllowed.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.AutoLevelUp.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.AdditionalAIBehaviors.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.CombatEncountersCapacity.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.SpaceCombatDifficulty.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.EnemyHitPointsPercentModifier.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.AllyResolveModifier.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		m_Settings.NPCAttributesBaseValuePercentModifier.OnValueChanged += delegate
		{
			OnSettingChanged();
		};
		SettingsController.Instance.OnConfirmedAllSettings += ApplyUpdatedSettings;
	}

	public void ResetDifficultyToDefault()
	{
		m_Settings.GameDifficulty.ResetToDefault();
		m_Settings.RespecAllowed.ResetToDefault();
		m_Settings.AutoLevelUp.ResetToDefault();
		m_Settings.AdditionalAIBehaviors.ResetToDefault();
		m_Settings.CombatEncountersCapacity.ResetToDefault();
	}

	private void OnSettingChanged()
	{
		m_HasUpdatedSettings = true;
	}

	private void ApplyUpdatedSettings()
	{
		if (!m_HasUpdatedSettings)
		{
			return;
		}
		m_HasUpdatedSettings = false;
		SettingsController.Instance.DifficultyPresetsController.UpdateGameDifficultiesComparisons();
		EventBus.RaiseEvent(delegate(IDifficultyChangedClassHandler h)
		{
			h.HandleDifficultyChanged();
		});
		if (ContextData<BlueprintUnitCheckerInEditorContextData>.Current != null)
		{
			return;
		}
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			allUnit.DifficultyChanged();
		}
	}

	public DifficultyPreset ExtractFromSettings()
	{
		return m_Settings.ToDifficultyPreset();
	}

	private DifficultyPreset ExtractFromTempSettings()
	{
		return m_Settings.TempToDifficultyPreset();
	}

	private void SetImmersiveMode(bool startGameImmersion)
	{
		GameCombatTextsSettings combatTexts = SettingsRoot.Game.CombatTexts;
		GameDialogsSettings dialogs = SettingsRoot.Game.Dialogs;
		if (startGameImmersion)
		{
			combatTexts.ShowSpellName.SetTempValue(EntitiesType.None);
			combatTexts.ShowAvoid.SetTempValue(EntitiesType.None);
			combatTexts.ShowMiss.SetTempValue(EntitiesType.None);
			combatTexts.ShowAttackOfOpportunity.SetTempValue(EntitiesType.None);
			combatTexts.ShowCriticalHit.SetTempValue(EntitiesType.None);
			combatTexts.ShowSneakAttack.SetTempValue(EntitiesType.None);
			combatTexts.ShowDamage.SetTempValue(EntitiesType.None);
			combatTexts.ShowSaves.SetTempValue(EntitiesType.None);
			dialogs.ShowItemsReceivedNotification.SetTempValue(value: false);
			dialogs.ShowLocationRevealedNotification.SetTempValue(value: false);
			dialogs.ShowXPGainedNotification.SetTempValue(value: false);
			dialogs.ShowAlignmentShiftsInAnswer.SetTempValue(value: false);
			dialogs.ShowAlignmentShiftsNotifications.SetTempValue(value: false);
			dialogs.ShowAlignmentRequirements.SetTempValue(value: false);
			dialogs.ShowSkillcheckDC.SetTempValue(value: false);
			dialogs.ShowSkillcheckResult.SetTempValue(value: false);
		}
		else
		{
			combatTexts.ShowSpellName.ResetToDefault();
			combatTexts.ShowAvoid.ResetToDefault();
			combatTexts.ShowMiss.ResetToDefault();
			combatTexts.ShowAttackOfOpportunity.ResetToDefault();
			combatTexts.ShowCriticalHit.ResetToDefault();
			combatTexts.ShowSneakAttack.ResetToDefault();
			combatTexts.ShowDamage.ResetToDefault();
			combatTexts.ShowSaves.ResetToDefault();
			dialogs.ShowItemsReceivedNotification.ResetToDefault();
			dialogs.ShowLocationRevealedNotification.ResetToDefault();
			dialogs.ShowXPGainedNotification.ResetToDefault();
			dialogs.ShowAlignmentShiftsInAnswer.ResetToDefault();
			dialogs.ShowAlignmentShiftsNotifications.ResetToDefault();
			dialogs.ShowAlignmentRequirements.ResetToDefault();
			dialogs.ShowSkillcheckDC.ResetToDefault();
			dialogs.ShowSkillcheckResult.ResetToDefault();
		}
	}
}
