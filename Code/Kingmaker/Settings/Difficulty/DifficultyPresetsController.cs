using System;
using System.Linq;
using Code.GameCore.Editor.Blueprints.BlueprintUnitEditorChecker;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.DisposableExtension;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Settings.Difficulty;

public class DifficultyPresetsController
{
	private readonly DifficultySettings m_Settings;

	private readonly DifficultyPreset[] m_Presets;

	private DifficultyPreset m_LastSetValues;

	private readonly DisposableBooleanFlag m_ApplyingPreset = new DisposableBooleanFlag();

	private readonly int[] m_DifficultiesComparisonsWithCurrent;

	public DifficultyPresetsController(DifficultyPresetsList difficultyPresetsList)
	{
		m_Settings = SettingsRoot.Difficulty;
		AddTempValueChangedAdditionalMethod(delegate
		{
			OnTempSettingChanged();
		});
		m_Presets = difficultyPresetsList.Difficulties.Select((DifficultyPresetAsset blueprint) => blueprint.Preset).ToArray();
		m_Settings.GameDifficulty.OnTempValueChanged += SetPreset;
		ApplyCurrentDifficultyPreset();
		m_DifficultiesComparisonsWithCurrent = new int[difficultyPresetsList.Difficulties.Count()];
		UpdateGameDifficultiesComparisons();
	}

	private void SetPreset(GameDifficultyOption difficulty)
	{
		if (difficulty != GameDifficultyOption.Custom)
		{
			DifficultyPreset difficultyPreset = m_Presets.FindOrDefault((DifficultyPreset p) => p.GameDifficulty == difficulty);
			if (difficultyPreset != null)
			{
				SetValues(difficultyPreset);
			}
		}
	}

	public void SetDifficultyPreset(DifficultyPreset values, bool confirm)
	{
		SetValues(values);
		if (confirm)
		{
			SettingsController.Instance.ConfirmAllTempValues();
		}
		UpdateGameDifficultiesComparisons();
	}

	private void SetValues(DifficultyPreset values)
	{
		m_LastSetValues = values;
		using (m_ApplyingPreset.Retain())
		{
			m_Settings.GameDifficulty.SetTempValue(values.GameDifficulty);
			m_Settings.RespecAllowed.SetTempValue(values.RespecAllowed);
			m_Settings.AutoLevelUp.SetTempValue(values.AutoLevelUp);
			m_Settings.AdditionalAIBehaviors.SetTempValue(values.AdditionalAIBehaviors);
			m_Settings.CombatEncountersCapacity.SetTempValue(values.CombatEncountersCapacity);
			m_Settings.EnemyDodgePercentModifier.SetTempValue(values.EnemyDodgePercentModifier);
			m_Settings.CoverHitBonusHalfModifier.SetTempValue(values.CoverHitBonusHalfModifier);
			m_Settings.CoverHitBonusFullModifier.SetTempValue(values.CoverHitBonusFullModifier);
			m_Settings.MinPartyDamage.SetTempValue(values.MinPartyDamage);
			m_Settings.MinPartyDamageFraction.SetTempValue(values.MinPartyDamageFraction);
			m_Settings.MinPartyStarshipDamage.SetTempValue(values.MinPartyStarshipDamage);
			m_Settings.MinPartyStarshipDamageFraction.SetTempValue(values.MinPartyStarshipDamageFraction);
			m_Settings.PartyMomentumPercentModifier.SetTempValue(values.PartyMomentumPercentModifier);
			m_Settings.NPCAttributesBaseValuePercentModifier.SetTempValue(values.NPCAttributesBaseValuePercentModifier);
			m_Settings.HardCrowdControlOnPartyMaxDurationRounds.SetTempValue(values.HardCrowdControlOnPartyMaxDurationRounds);
			m_Settings.SkillCheckModifier.SetTempValue(values.SkillCheckModifier);
			m_Settings.EnemyHitPointsPercentModifier.SetTempValue(values.EnemyHitPointsPercentModifier);
			m_Settings.AllyResolveModifier.SetTempValue(values.AllyResolveModifier);
			m_Settings.PartyDamageDealtAfterArmorReductionPercentModifier.SetTempValue(values.PartyDamageDealtAfterArmorReductionPercentModifier);
			m_Settings.WoundDamagePerTurnThresholdHPFraction.SetTempValue(values.WoundDamagePerTurnThresholdHPFraction);
			m_Settings.OldWoundDelayRounds.SetTempValue(values.OldWoundDelayRounds);
			m_Settings.WoundStacksForTrauma.SetTempValue(values.WoundStacksForTrauma);
			m_Settings.SpaceCombatDifficulty.SetTempValue(values.SpaceCombatDifficulty);
		}
	}

	private void AddTempValueChangedAdditionalMethod(Action method)
	{
		m_Settings.RespecAllowed.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.AutoLevelUp.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.AdditionalAIBehaviors.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.CombatEncountersCapacity.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyDodgePercentModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.CoverHitBonusHalfModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.CoverHitBonusFullModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.MinPartyDamage.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.MinPartyDamageFraction.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.MinPartyStarshipDamage.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.MinPartyStarshipDamageFraction.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.PartyMomentumPercentModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.NPCAttributesBaseValuePercentModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.HardCrowdControlOnPartyMaxDurationRounds.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.SkillCheckModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.EnemyHitPointsPercentModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.AllyResolveModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.PartyDamageDealtAfterArmorReductionPercentModifier.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.WoundDamagePerTurnThresholdHPFraction.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.OldWoundDelayRounds.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.WoundStacksForTrauma.OnTempValueChanged += delegate
		{
			method();
		};
		m_Settings.SpaceCombatDifficulty.OnTempValueChanged += delegate
		{
			method();
		};
	}

	private void OnTempSettingChanged()
	{
		if (ContextData<BlueprintUnitCheckerInEditorContextData>.Current != null || (bool)m_ApplyingPreset || m_LastSetValues == null)
		{
			return;
		}
		if (m_LastSetValues.CompareTo(ExtractFromTempSettings()) == 0)
		{
			m_Settings.GameDifficulty.SetTempValue(m_LastSetValues.GameDifficulty);
			return;
		}
		m_Settings.GameDifficulty.SetTempValue(GameDifficultyOption.Custom);
		m_Presets.FindOrDefault((DifficultyPreset p) => p.GameDifficulty == GameDifficultyOption.Custom);
	}

	private DifficultyPreset ExtractFromSettings()
	{
		return m_Settings.ToDifficultyPreset();
	}

	private DifficultyPreset ExtractFromTempSettings()
	{
		return m_Settings.TempToDifficultyPreset();
	}

	public void UpdateGameDifficultiesComparisons()
	{
		DifficultyPreset difficultyPreset = ExtractFromSettings();
		for (int i = 0; i < m_DifficultiesComparisonsWithCurrent.Length; i++)
		{
			m_DifficultiesComparisonsWithCurrent[i] = difficultyPreset.CompareTo(m_Presets[i]);
		}
	}

	public void ApplyCurrentDifficultyPreset()
	{
		if ((GameDifficultyOption)m_Settings.GameDifficulty != GameDifficultyOption.Custom)
		{
			SetPreset(m_Settings.GameDifficulty);
		}
	}

	public int CurrentDifficultyCompareTo(GameDifficultyOption gameDifficultyOption)
	{
		return m_DifficultiesComparisonsWithCurrent[EnumUtils.GetOrdinalNumber(gameDifficultyOption)];
	}
}
