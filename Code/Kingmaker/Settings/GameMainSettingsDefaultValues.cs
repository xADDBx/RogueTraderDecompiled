using System;
using Kingmaker.Localization.Enums;

namespace Kingmaker.Settings;

[Serializable]
public class GameMainSettingsDefaultValues
{
	public Locale Localization;

	public bool AutofillActionbarSlots;

	public bool LootInCombat;

	public bool SendGameStatistic;

	public bool SendSaves;

	public bool UseHotAreas;

	public bool BloodOnCharacters;

	public bool DismemberCharacters;

	public bool AcceleratedMove;

	public MainMenuTheme MainMenuTheme;
}
