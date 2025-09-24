using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.CounterWindow;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public static class VendorHelper
{
	public enum SaleOptions
	{
		MasterWork,
		NonMagical,
		GemsAnimalParts
	}

	public static VendorLogic Vendor => Game.Instance.Vendor;

	public static void TryMove(ItemEntity itemEntity, ItemsCollection collection, bool split)
	{
		if (!Vendor.IsTrading)
		{
			return;
		}
		if (collection == Vendor.StoreItems)
		{
			EventBus.RaiseEvent(delegate(IVendorTransferHandler h)
			{
				h.HandleTransitionWindow(itemEntity);
			});
		}
		else if (collection == Vendor.ItemsForBuy)
		{
			TryMoveSplit(itemEntity, split, delegate(int count)
			{
				Game.Instance.GameCommandQueue.RemoveFromBuyVendor(itemEntity, count);
			});
		}
	}

	public static void VendorTryBuyAllAvailable(Action availabilityCheck)
	{
		List<ItemEntity> items = GetAvailableItemsList();
		if (HasItemsToBuy())
		{
			EventBus.RaiseEvent(delegate(IVendorMultipleTransferHandler h)
			{
				h.HandleTransitionWindow(items, availabilityCheck);
			});
		}
	}

	public static bool HasItemsToBuy()
	{
		return GetAvailableItemsList().Any();
	}

	private static List<ItemEntity> GetAvailableItemsList()
	{
		List<ItemEntity> list = (from item in Vendor.StoreItems
			orderby Game.Instance.Vendor.VendorInventory.GetReputationToUnlock(item)
			where !Game.Instance.Vendor.VendorInventory.IsLockedByReputation(item)
			select item).ToList();
		List<ItemEntity> list2 = new List<ItemEntity>();
		foreach (ItemEntity item in list)
		{
			if (Game.Instance.Vendor.GetItemBuyPrice(item) <= Game.Instance.Player.ProfitFactor.Total)
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	private static void BuyItemsList(List<ItemEntity> itemEntitiesCanBuy)
	{
		Game.Instance.Vendor.MakeDealWithCurrentVendor();
		UISounds.Instance.Sounds.Vendor.Deal.Play();
	}

	public static void TryMoveSplit(ItemEntity itemEntity, bool split, Action<int> command)
	{
		if (Vendor.VendorInventory.IsLockedByReputation(itemEntity))
		{
			PFLog.UI.Log("Item {0} locked by reputation {1}/{2}", itemEntity.Name, Vendor.VendorInventory.GetCurrentFactionReputationPoints(), Vendor.VendorInventory.GetReputationToUnlock(itemEntity));
		}
		else if (split && itemEntity.Count > 1)
		{
			EventBus.RaiseEvent(delegate(ICounterWindowUIHandler h)
			{
				h.HandleOpen(CounterWindowType.Move, itemEntity, command.Invoke);
			});
		}
		else
		{
			command(1);
		}
	}
}
