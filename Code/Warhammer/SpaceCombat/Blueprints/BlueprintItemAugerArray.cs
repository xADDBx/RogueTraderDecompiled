using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("10e0c4e16b29cf24ba962c87a49146d8")]
public class BlueprintItemAugerArray : BlueprintStarshipItem
{
	[Header("Attack Bonuses")]
	[Range(0f, 100f)]
	public int hitChances;

	[Range(0f, 100f)]
	public int critChances;

	public override ItemsItemType ItemType => ItemsItemType.StarshipAugerArray;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
