using System;
using Kingmaker.Code.UI.MVVM.VM.CounterWindow;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;

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
