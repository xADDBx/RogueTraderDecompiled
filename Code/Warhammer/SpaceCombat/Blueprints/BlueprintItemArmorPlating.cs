using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("f6ca6d5da1433d44a93b03a4c9bded9e")]
public class BlueprintItemArmorPlating : BlueprintStarshipItem
{
	public int ArmourFore;

	public int ArmourPort;

	public int ArmourStarboard;

	public int ArmourAft;

	public override ItemsItemType ItemType => ItemsItemType.StarshipArmorPlating;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
