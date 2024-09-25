using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIColonizationTexts
{
	[Serializable]
	public struct ColonyResourceStrings
	{
		public LocalizedString Name;

		public LocalizedString Description;
	}

	[Serializable]
	public struct ColonyStatsStrings
	{
		public LocalizedString Name;

		public LocalizedString Description;
	}

	public ColonyResourceStrings[] ResourceStrings;

	public ColonyStatsStrings[] StatStrings;

	public LocalizedString ColonyStatsTitle;

	public LocalizedString ColonyStatModifierOriginOther;

	public LocalizedString ColonyManagementNoColonies;

	public LocalizedString ColonyManagementVisitColonyButton;

	public ColonyResourceStrings GetResourceStrings(int index)
	{
		if (index > ResourceStrings.Length)
		{
			return default(ColonyResourceStrings);
		}
		return ResourceStrings[index];
	}

	public ColonyStatsStrings GetStatStrings(int index)
	{
		if (index > StatStrings.Length)
		{
			return default(ColonyStatsStrings);
		}
		return StatStrings[index];
	}
}
