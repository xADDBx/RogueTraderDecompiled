using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("5896161b9357eb64f9bba48be98e1bf7")]
public class BlueprintItemLifeSustainer : BlueprintStarshipItem
{
	public override ItemsItemType ItemType => ItemsItemType.StarshipLifeSustainer;

	public override string InventoryEquipSound
	{
		get
		{
			throw new NotImplementedException();
		}
	}
}
