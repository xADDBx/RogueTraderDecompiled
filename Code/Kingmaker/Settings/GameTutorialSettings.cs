using System;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Tutorial;

namespace Kingmaker.Settings;

public class GameTutorialSettings
{
	public readonly SettingsEntityBool ShowAllTutorials;

	public readonly SettingsEntityBool ShowBasicTutorial;

	public readonly SettingsEntityBool ShowControlsBasicTutorial;

	public readonly SettingsEntityBool ShowControlsAdvancedTutorial;

	public readonly SettingsEntityBool ShowGameplayBasicTutorial;

	public readonly SettingsEntityBool ShowGameplayAdvancedTutorial;

	public readonly SettingsEntityBool ShowWarhammerRulesTutorial;

	public readonly SettingsEntityBool ShowSpaceEncountersTutorial;

	public readonly SettingsEntityBool ShowSpaceExplorationTutorial;

	public readonly SettingsEntityBool ShowGroundEncountersTutorial;

	public readonly SettingsEntityBool ShowContextTutorial;

	public readonly SettingsEntityBool ShowSpecialLoot;

	public GameTutorialSettings(ISettingsController settingsController, GameTutorialSettingsDefaultValues defaultValues)
	{
		ShowAllTutorials = new SettingsEntityBool(settingsController, "all-tutorials", defaultValues.ShowAllTutorials);
		ShowBasicTutorial = new SettingsEntityBool(settingsController, "basis", defaultValues.ShowBasicTutorial);
		ShowControlsBasicTutorial = new SettingsEntityBool(settingsController, "controls-basis", defaultValues.ShowControlsBasicTutorial);
		ShowControlsAdvancedTutorial = new SettingsEntityBool(settingsController, "controls-advanced", defaultValues.ShowControlsAdvancedTutorial);
		ShowGameplayBasicTutorial = new SettingsEntityBool(settingsController, "gameplay-basic", defaultValues.ShowGameplayBasicTutorial);
		ShowGameplayAdvancedTutorial = new SettingsEntityBool(settingsController, "gameplay-advanced", defaultValues.ShowGameplayAdvancedTutorial);
		ShowWarhammerRulesTutorial = new SettingsEntityBool(settingsController, "warhammer-rules", defaultValues.ShowWarhammerRulesTutorial);
		ShowSpaceEncountersTutorial = new SettingsEntityBool(settingsController, "space-encounters", defaultValues.ShowSpaceEncountersTutorial);
		ShowSpaceExplorationTutorial = new SettingsEntityBool(settingsController, "space-exploration", defaultValues.ShowSpaceExplorationTutorial);
		ShowGroundEncountersTutorial = new SettingsEntityBool(settingsController, "ground-encounters", defaultValues.ShowGroundEncountersTutorial);
		ShowContextTutorial = new SettingsEntityBool(settingsController, "context", defaultValues.ShowContextTutorial);
		ShowSpecialLoot = new SettingsEntityBool(settingsController, "special-loot", defaultValues.ShowSpecialLoot);
	}

	public void SetValueAndConfirmForAll(bool value)
	{
		ForEach(delegate(SettingsEntityBool x)
		{
			x.SetValueAndConfirm(value);
		});
	}

	public bool ShouldShowTag(TutorialTag tutorialTag)
	{
		SettingsEntityBool settingByTag = GetSettingByTag(tutorialTag);
		if (settingByTag == null)
		{
			return true;
		}
		return settingByTag;
	}

	public void SetTempValueForTag(TutorialTag tutorialTag, bool value)
	{
		GetSettingByTag(tutorialTag).SetTempValue(value);
	}

	public void SetValueAndConfirmForTag(TutorialTag tutorialTag, bool value)
	{
		GetSettingByTag(tutorialTag)?.SetValueAndConfirm(value);
	}

	public void ForEach(Action<SettingsEntityBool> settingsAction)
	{
		if (settingsAction != null)
		{
			settingsAction(ShowAllTutorials);
			settingsAction(ShowBasicTutorial);
			settingsAction(ShowControlsBasicTutorial);
			settingsAction(ShowControlsAdvancedTutorial);
			settingsAction(ShowGameplayBasicTutorial);
			settingsAction(ShowGameplayAdvancedTutorial);
			settingsAction(ShowWarhammerRulesTutorial);
			settingsAction(ShowSpaceEncountersTutorial);
			settingsAction(ShowSpaceExplorationTutorial);
			settingsAction(ShowGroundEncountersTutorial);
			settingsAction(ShowContextTutorial);
		}
	}

	private SettingsEntityBool GetSettingByTag(TutorialTag tutorialTag)
	{
		return tutorialTag switch
		{
			TutorialTag.Basic => ShowBasicTutorial, 
			TutorialTag.ControlsBasic => ShowControlsBasicTutorial, 
			TutorialTag.ControlsAdvanced => ShowControlsAdvancedTutorial, 
			TutorialTag.GameplayBasic => ShowGameplayBasicTutorial, 
			TutorialTag.GameplayAdvanced => ShowGameplayAdvancedTutorial, 
			TutorialTag.WarhammerRules => ShowWarhammerRulesTutorial, 
			TutorialTag.SpaceEncounters => ShowSpaceEncountersTutorial, 
			TutorialTag.SpaceExploration => ShowSpaceExplorationTutorial, 
			TutorialTag.GroundEncounters => ShowGroundEncountersTutorial, 
			TutorialTag.SpecialLoot => ShowSpecialLoot, 
			_ => null, 
		};
	}
}
