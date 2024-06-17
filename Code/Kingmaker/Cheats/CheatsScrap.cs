using System;
using System.Linq;
using Core.Cheats;
using Kingmaker.Items;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.StarshipLogic;

namespace Kingmaker.Cheats;

internal static class CheatsScrap
{
	[Cheat(Name = "scrap", Description = "Show amount of scrap that player has", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static int Scrap()
	{
		return Game.Instance.Player.Scrap;
	}

	[Cheat(Name = "scrap_receive", Description = "Give scrap to player", ExampleArgs = "Any positive integer: 20; 32, 265", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Receive(int scrap)
	{
		Game.Instance.Player.Scrap.Receive(scrap);
	}

	[Cheat(Name = "scrap_spend", Description = "Take scrap from player", ExampleArgs = "Any positive integer: 20; 32, 265", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void Spend(int scrap)
	{
		Game.Instance.Player.Scrap.Spend(scrap);
	}

	[Cheat(Name = "scrap_repair_full", Description = "Repair players ship for scrap on max HP", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RepairShipFull()
	{
		Game.Instance.Player.Scrap.RepairShipFull();
	}

	[Cheat(Name = "scrap_repair", Description = "Repair players ship for scrap", ExampleArgs = "Amount of health to restore in integer: 7, 12, 30", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RepairShip(int restoreHealth)
	{
		Game.Instance.Player.Scrap.RepairShip(restoreHealth);
	}

	[Cheat(Name = "scrap_disassemble_component", Description = "Disassemble component of ship into scrap", ExampleArgs = "Index in inventory(from 0): 0, 2, 12", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string DisassemblePart(int index)
	{
		ItemEntity itemEntity = Game.Instance.Player.Inventory.Items.Where((ItemEntity x) => x.IsInStash && x.CanBeDisassembled).ElementAtOrDefault(index);
		if (itemEntity == null)
		{
			return "Can't find component";
		}
		if (!itemEntity.Disassemble())
		{
			return "Disassemble failed";
		}
		return "Scrap received";
	}

	[Cheat(Name = "scrap_assemble_component", Description = "Tries to assemble component of ship into scrap", ExampleArgs = "Index in inventory(from 0): 0, 2, 12", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string AssemblePart(int index)
	{
		ItemEntity itemEntity = Game.Instance.Player.Inventory.Items.Where((ItemEntity x) => x.IsInStash && x.CanBeAssembled).ElementAtOrDefault(index);
		if (itemEntity == null)
		{
			return "Can't find component";
		}
		if (!itemEntity.Assemble())
		{
			return "Assemble failed";
		}
		return "Component Assembled";
	}

	private static Type TypeCheck(ItemEntity item)
	{
		if (!(item is StarshipItemEntity<BlueprintStarshipWeapon>))
		{
			if (!(item is StarshipItemEntity<BlueprintItemVoidShieldGenerator>))
			{
				if (item is StarshipItemEntity<BlueprintItemPlasmaDrives>)
				{
					return typeof(StarshipItemEntity<BlueprintItemPlasmaDrives>);
				}
				return typeof(StarshipItemEntity<BlueprintStarshipItem>);
			}
			return typeof(StarshipItemEntity<BlueprintItemVoidShieldGenerator>);
		}
		return typeof(StarshipItemEntity<BlueprintStarshipWeapon>);
	}
}
