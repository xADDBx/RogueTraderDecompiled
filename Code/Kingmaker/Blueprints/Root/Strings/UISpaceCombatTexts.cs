using System;
using Kingmaker.Localization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UISpaceCombatTexts
{
	[Serializable]
	public struct SpacePostStrings
	{
		public LocalizedString Title;

		public LocalizedString Description;
	}

	public LocalizedString BackToShipBridge;

	public LocalizedString ExitBattle;

	public LocalizedString ArmorHint;

	public LocalizedString ShieldsHint;

	public LocalizedString Crew;

	public LocalizedString Morale;

	public LocalizedString MilitaryRating;

	public LocalizedString CombatMovementActionHint;

	public LocalizedString TimeSurvivalHint;

	public LocalizedString TimeSurvivalActionHint;

	public LocalizedString ShipMovementWarning;

	public LocalizedString PortAbilitiesGroupLabel;

	public LocalizedString ProwAbilitiesGroupLabel;

	public LocalizedString DorsalAbilitiesGroupLabel;

	public LocalizedString StarboardAbilitiesGroupLabel;

	public LocalizedString NavigatorResource;

	public LocalizedString NavigatorResourceDescription;

	public LocalizedString TorpedoSelfDestruct;

	public SpacePostStrings[] PostStrings;

	public LocalizedString KoronusExpanse;

	public SpacePostStrings GetPostStrings(int index)
	{
		if (index > PostStrings.Length)
		{
			return default(SpacePostStrings);
		}
		return PostStrings[index];
	}
}
