using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("427d75a81e7bb9a40ac9b7eed773b946")]
public class BlueprintItemVoidShieldGenerator : BlueprintStarshipItem
{
	[Header("Generating Shields")]
	public int Fore;

	public int Port;

	public int Starboard;

	public int Aft;

	[Header("Special resistance to damage type (Direct for none)")]
	public DamageType damageExtraResistance = DamageType.Direct;

	[ShowIf("HasExtraResistance")]
	public int extraResistanceDamageReductionPercent;

	[Header("Shields are not spent when ramming")]
	public bool offOnRam;

	public bool HasExtraResistance => damageExtraResistance != DamageType.Direct;

	public override ItemsItemType ItemType => ItemsItemType.StarshipVoidShieldGenerator;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
