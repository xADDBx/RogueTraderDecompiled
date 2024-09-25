using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Settings;
using Kingmaker.UI.Models.SettingsUI.SettingAssets;

namespace Kingmaker.UI.Models.SettingsUI.UISettingsSheet;

[Serializable]
public class UIGameTutorialSettings : IUISettingsSheet
{
	public UISettingsEntityBool ShowAllTutorials;

	public UISettingsEntityBool ShowBasicTutorial;

	public UISettingsEntityBool ShowControlsBasicTutorial;

	public UISettingsEntityBool ShowControlsAdvancedTutorial;

	public UISettingsEntityBool ShowGameplayBasicTutorial;

	public UISettingsEntityBool ShowGameplayAdvancedTutorial;

	public UISettingsEntityBool ShowWarhammerRulesTutorial;

	public UISettingsEntityBool ShowSpaceEncountersTutorial;

	public UISettingsEntityBool ShowSpaceExplorationTutorial;

	public UISettingsEntityBool ShowGroundEncountersTutorial;

	public UISettingsEntityBool ShowContextTutorial;

	public UISettingsEntityBool ShowSpecialLoot;

	public void LinkToSettings()
	{
		ShowAllTutorials.LinkSetting(SettingsRoot.Game.Tutorial.ShowAllTutorials);
		ShowBasicTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowBasicTutorial);
		ShowControlsBasicTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowControlsBasicTutorial);
		ShowControlsAdvancedTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowControlsAdvancedTutorial);
		ShowGameplayBasicTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowGameplayBasicTutorial);
		ShowGameplayAdvancedTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowGameplayAdvancedTutorial);
		ShowWarhammerRulesTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowWarhammerRulesTutorial);
		ShowSpaceEncountersTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowSpaceEncountersTutorial);
		ShowSpaceExplorationTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowSpaceExplorationTutorial);
		ShowGroundEncountersTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowGroundEncountersTutorial);
		ShowContextTutorial.LinkSetting(SettingsRoot.Game.Tutorial.ShowContextTutorial);
		ShowSpecialLoot.LinkSetting(SettingsRoot.Game.Tutorial.ShowSpecialLoot);
	}

	public void InitializeSettings()
	{
		UpdateInteractable(string.Empty);
	}

	public void UpdateInteractable(string key)
	{
		if (key == string.Empty)
		{
			return;
		}
		if (key == ShowAllTutorials.Setting?.Key)
		{
			GetAllTutorials().ForEach(delegate(UISettingsEntityBool t)
			{
				t.SetTempValue(ShowAllTutorials.GetTempValue());
			});
		}
		else if (!GetAllTutorials().All((UISettingsEntityBool t) => t.Setting?.Key != key))
		{
			if (GetAllTutorials().All((UISettingsEntityBool t) => t.GetTempValue()))
			{
				ShowAllTutorials.SetTempValue(value: true);
			}
			else if (GetAllTutorials().All((UISettingsEntityBool t) => !t.GetTempValue()))
			{
				ShowAllTutorials.SetTempValue(value: false);
			}
		}
	}

	private List<UISettingsEntityBool> GetAllTutorials()
	{
		return new List<UISettingsEntityBool> { ShowBasicTutorial, ShowControlsBasicTutorial, ShowControlsAdvancedTutorial, ShowGameplayBasicTutorial, ShowGameplayAdvancedTutorial, ShowWarhammerRulesTutorial, ShowSpaceEncountersTutorial, ShowSpaceExplorationTutorial, ShowGroundEncountersTutorial, ShowContextTutorial };
	}
}
