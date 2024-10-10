using Kingmaker.Settings.ConstructionHelpers.KeyPrefix;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameSettings
{
	public readonly GameMainSettings Main;

	public readonly GameTutorialSettings Tutorial;

	public readonly GameSaveSettings Save;

	public readonly GameTooltipsSettings Tooltips;

	public readonly GameMainMenuSettings MainMenu;

	public readonly GameCombatTextsSettings CombatTexts;

	public readonly GameDialogsSettings Dialogs;

	public readonly GameAutopauseSettings Autopause;

	public readonly GameTurnBasedSettings TurnBased;

	public GameSettings(ISettingsController settingsController, GameSettingsDefaultValues defaultValues)
	{
		using (new SettingsKeyPrefix("main"))
		{
			Main = new GameMainSettings(settingsController, defaultValues.Main);
		}
		using (new SettingsKeyPrefix("tutorial"))
		{
			Tutorial = new GameTutorialSettings(settingsController, defaultValues.Tutorial);
		}
		GameSaveSettingsDefaultValues save = defaultValues.Save;
		using (new SettingsKeyPrefix("save"))
		{
			Save = new GameSaveSettings(settingsController, save);
		}
		using (new SettingsKeyPrefix("tooltips"))
		{
			Tooltips = new GameTooltipsSettings(settingsController, defaultValues.Tooltips);
		}
		using (new SettingsKeyPrefix("main-menu"))
		{
			MainMenu = new GameMainMenuSettings(settingsController, defaultValues.MainMenu);
		}
		using (new SettingsKeyPrefix("combat-texts"))
		{
			CombatTexts = new GameCombatTextsSettings(settingsController, defaultValues.CombatTexts);
		}
		using (new SettingsKeyPrefix("dialogs"))
		{
			Dialogs = new GameDialogsSettings(settingsController, defaultValues.Dialogs);
		}
		using (new SettingsKeyPrefix("autopause"))
		{
			Autopause = new GameAutopauseSettings(settingsController, defaultValues.Autopause);
		}
		using (new SettingsKeyPrefix("turn-based"))
		{
			TurnBased = new GameTurnBasedSettings(settingsController, defaultValues.TurnBased);
		}
	}
}
