using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Serializable]
[TypeId("7ba7d7f47b052584f94c332181e45805")]
public class FixCompanionSharedInventory : PlayerUpgraderOnlyAction
{
	public override string GetCaption()
	{
		return "WH-474446: restore shared party inventory for player companions stuck with their own inventory";
	}

	protected override void RunActionOverride()
	{
		foreach (BaseUnitEntity allCharacter in Game.Instance.Player.AllCharacters)
		{
			allCharacter.GetInventoryOptional()?.RestoreSharedInventory();
		}
	}
}
