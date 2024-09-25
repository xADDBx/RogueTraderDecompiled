using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("fdac0d09c25704d448daaccbfc121d94")]
public class BlueprintItemGellerFieldDevice : BlueprintStarshipItem
{
	public override ItemsItemType ItemType => ItemsItemType.StarshipGellerFieldDevice;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
