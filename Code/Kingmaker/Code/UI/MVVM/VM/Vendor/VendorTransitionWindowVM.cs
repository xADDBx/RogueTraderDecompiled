using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorTransitionWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVendorDealHandler, ISubscriber
{
	private readonly Action m_Close;

	public readonly int MaxValue;

	public int CurrentValue;

	public readonly ItemSlotVM Slot;

	public readonly ItemSlotsGroupVM Slots;

	public VendorTransitionWindowVM(VendorLogic vendor, ItemEntity itemEntity, Action close)
	{
		m_Close = close;
		Slots = null;
		AddDisposable(Slot = new ItemSlotVM(itemEntity, 0));
		if (itemEntity != null)
		{
			MaxValue = itemEntity.Count;
		}
		if (itemEntity != null)
		{
			CurrentValue = Mathf.Max(itemEntity.Count / 2, 1);
		}
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		AddDisposable(EventBus.Subscribe(this));
	}

	public VendorTransitionWindowVM(VendorLogic vendor, List<ItemEntity> itemEntities, Action close)
	{
		m_Close = close;
		Slot = null;
		ItemsCollection collection = new ItemsCollection(null);
		for (int i = 0; i < itemEntities.Count; i++)
		{
			itemEntities[i].SetSlotIndex(i);
		}
		int num = Mathf.Min(itemEntities.Count, 6);
		AddDisposable(Slots = new ItemSlotsGroupVM(collection, itemEntities, num, num, needMaximumLimit: true));
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
	}

	public void Deal()
	{
		if (Slots != null)
		{
			foreach (ItemEntity item in Slots.Items)
			{
				Game.Instance.GameCommandQueue.AddForBuyVendor(item, item.Count, makeDeal: true);
				UISounds.Instance.Sounds.Vendor.Deal.Play();
			}
			return;
		}
		if (Slot == null)
		{
			Close();
			return;
		}
		Game.Instance.GameCommandQueue.AddForBuyVendor(Slot.ItemEntity, CurrentValue, makeDeal: true);
		UISounds.Instance.Sounds.Vendor.Deal.Play();
	}

	public void Close()
	{
		m_Close?.Invoke();
	}

	public void HandleVendorDeal()
	{
		Close();
	}

	public void HandleCancelVendorDeal()
	{
	}
}
