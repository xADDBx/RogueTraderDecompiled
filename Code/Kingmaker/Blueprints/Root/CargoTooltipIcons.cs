using System;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
public class CargoTooltipIcons
{
	public Sprite Xeno;

	public Sprite Chaos;

	public Sprite ShipComponents;

	public Sprite RangedWeaponry;

	public Sprite MeleeWeaponry;

	public Sprite Tech;

	public Sprite Textile;

	public Sprite Armours;

	public Sprite Provision;

	public Sprite Fuel;

	public Sprite Holy;

	public Sprite Jewelry;

	public Sprite Miscellaneous;

	public Sprite People;

	public Sprite Torpedoes;

	public Sprite SpacePirates;

	public Sprite SpaceChaos;

	public Sprite SpaceAeldari;

	public Sprite SpaceDrukhari;

	public Sprite SpaceNecrons;

	public Sprite GetIconByOrigin(ItemsItemOrigin origin)
	{
		return origin switch
		{
			ItemsItemOrigin.None => null, 
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
			ItemsItemOrigin.EnergyBattery => Fuel, 
			ItemsItemOrigin.Transuranium => Fuel, 
			ItemsItemOrigin.Jewelry => Jewelry, 
			ItemsItemOrigin.Miscellaneous => Miscellaneous, 
			ItemsItemOrigin.People => People, 
			ItemsItemOrigin.Torpedoes => Torpedoes, 
			ItemsItemOrigin.SpacePirates => SpacePirates, 
			ItemsItemOrigin.SpaceChaos => SpaceChaos, 
			ItemsItemOrigin.SpaceAeldari => SpaceAeldari, 
			ItemsItemOrigin.SpaceDrukhari => SpaceDrukhari, 
			ItemsItemOrigin.SpaceNecrons => SpaceNecrons, 
			_ => null, 
		};
	}
}
