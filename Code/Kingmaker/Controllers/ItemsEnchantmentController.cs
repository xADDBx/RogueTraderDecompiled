using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;

namespace Kingmaker.Controllers;

public class ItemsEnchantmentController : IControllerTick, IController
{
	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		TickInventory(Game.Instance.Player.Inventory);
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			if (!allBaseAwakeUnit.Inventory.IsPlayerInventory)
			{
				TickInventory(allBaseAwakeUnit.Inventory.Collection);
			}
		}
	}

	private static void TickInventory(ItemsCollection inventory)
	{
		foreach (ItemEntity item in inventory.Items)
		{
			TickItem(item);
		}
	}

	private static void TickItem(ItemEntity item)
	{
		if (item == null)
		{
			return;
		}
		foreach (ItemEnchantment enchantment in item.Enchantments)
		{
			if (enchantment.IsEnded)
			{
				item.RemoveEnchantment(enchantment);
			}
		}
	}
}
