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

	public static UIGameOverScreen Instance => UIStrings.Instance.GameOverScreen;
}
