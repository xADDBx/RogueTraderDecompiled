using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIGameOverScreen
{
	public LocalizedString MaleDeadLabel;

	public LocalizedString FemaleDeadLabel;

	public LocalizedString PartyIsDefeatedLabel;

	public LocalizedString QuestIsFailedLabel;

	public LocalizedString GameOverIronManDescription;

	public LocalizedString QuickLoadLabel;

	public LocalizedString LoadLabel;

	public LocalizedString MainMenuLabel;

	public LocalizedString IronManDeleteSaveLabel;

	public LocalizedString IronManContinueGameLabel;

	public static UIGameOverScreen Instance => UIStrings.Instance.GameOverScreen;
}
