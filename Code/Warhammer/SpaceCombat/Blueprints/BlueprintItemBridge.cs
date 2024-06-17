using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("a9c2da5cdf1e96545b8b1932077442c7")]
public class BlueprintItemBridge : BlueprintStarshipItem
{
	public override ItemsItemType ItemType => ItemsItemType.StarshipBridge;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
