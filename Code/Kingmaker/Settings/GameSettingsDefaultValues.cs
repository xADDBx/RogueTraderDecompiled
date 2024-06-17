using System;
using JetBrains.Annotations;

namespace Kingmaker.Settings;

[Serializable]
public class GameSettingsDefaultValues
{
	public GameMainSettingsDefaultValues Main;

	public GameTutorialSettingsDefaultValues Tutorial;

	[UsedImplicitly]
	public GameSaveSettingsDefaultValues Save;

	[UsedImplicitly]
	public GameSaveSettingsDefaultValues SaveConsole;

	public GameTooltipsSettingsDefaultValues Tooltips;

	public GameCombatTextsSettingsDefaultValues CombatTexts;

	public GameDialogsSettingsDefaultValues Dialogs;

	public GameAutopauseSettingsDefaultValues Autopause;

	public GameTurnBasedSettingsDefaultValues TurnBased;

	public GameSillyCheatCodesSettingsDefaultValues SillyCheatCodes;
}
