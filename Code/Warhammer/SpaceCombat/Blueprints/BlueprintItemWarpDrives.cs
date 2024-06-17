using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("9a24abe68b113a649a9daec8b27a4357")]
public class BlueprintItemWarpDrives : BlueprintStarshipItem
{
	public override ItemsItemType ItemType => ItemsItemType.StarshipWarpDrives;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
