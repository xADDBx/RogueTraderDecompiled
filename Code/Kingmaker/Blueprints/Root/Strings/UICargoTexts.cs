using System;
using Kingmaker.Localization;
using Kingmaker.UI.Common;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UICargoTexts
{
	public LocalizedString Xeno;

	public LocalizedString Chaos;

	public LocalizedString ShipComponents;

	public LocalizedString RangedWeaponry;

	public LocalizedString MeleeWeaponry;

	public LocalizedString Tech;

	public LocalizedString Textile;

	public LocalizedString Armours;

	public LocalizedString Provision;

	public LocalizedString Fuel;

	public LocalizedString Holy;

	public LocalizedString EnergyBattery;

	public LocalizedString Transuranium;

	public LocalizedString Jewelry;

	public LocalizedString Miscellaneous;

	public LocalizedString People;

	public LocalizedString Torpedoes;

	public LocalizedString SpacePirates;

	public LocalizedString SpaceChaos;

	public LocalizedString SpaceAeldari;

	public LocalizedString SpaceDrukhari;

	public LocalizedString SpaceNecrons;

	public LocalizedString CargoUnusableFillValue;

	public LocalizedString CargoUnusableFill;

	public LocalizedString CargoTotalFill;

	public LocalizedString TooltipShowDetails;

	public LocalizedString CargoReceived;

	public LocalizedString CargoCreated;

	public LocalizedString Cargo;

	public LocalizedString CargoRewardsHeader;

	public LocalizedString CargoList;

	public LocalizedString EmptyCargo;

	public LocalizedString TrashItemCargo;

	public string GetLabelByOrigin(ItemsItemOrigin origin)
	{
		return origin switch
		{
			ItemsItemOrigin.Xeno => Xeno, 
			ItemsItemOrigin.Chaos => Chaos, 
			ItemsItemOrigin.ShipComponents => ShipComponents, 
			ItemsItemOrigin.RangedWeaponry => RangedWeaponry, 
			ItemsItemOrigin.MeleeWeaponry => MeleeWeaponry, 
			ItemsItemOrigin.Tech => Tech, 
			ItemsItemOrigin.Textile => Textile, 
			ItemsItemOrigin.Armours => Armours, 
			ItemsItemOrigin.Provisions => Provision, 
			ItemsItemOrigin.Fuel => Fuel, 
			ItemsItemOrigin.Holy => Holy, 
			ItemsItemOrigin.EnergyBattery => EnergyBattery, 
			ItemsItemOrigin.Transuranium => Transuranium, 
			ItemsItemOrigin.Jewelry => Jewelry, 
			ItemsItemOrigin.Miscellaneous => Miscellaneous, 
			ItemsItemOrigin.People => People, 
			ItemsItemOrigin.Torpedoes => Torpedoes, 
			ItemsItemOrigin.SpacePirates => SpacePirates, 
			ItemsItemOrigin.SpaceChaos => SpaceChaos, 
			ItemsItemOrigin.SpaceAeldari => SpaceAeldari, 
			ItemsItemOrigin.SpaceDrukhari => SpaceDrukhari, 
			ItemsItemOrigin.SpaceNecrons => SpaceNecrons, 
			_ => string.Empty, 
		};
	}
}
