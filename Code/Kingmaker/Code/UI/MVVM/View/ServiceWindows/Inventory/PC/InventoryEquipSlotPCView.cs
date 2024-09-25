using System;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.PC;

public class InventoryEquipSlotPCView : InventoryEquipSlotView
{
	[Space]
	[SerializeField]
	private ItemSlotPCView m_ItemSlotPCView;

	protected override void BindViewImplementation()
	{
		m_ItemSlotPCView.Bind(base.ViewModel);
		base.BindViewImplementation();
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(OnClick));
		AddDisposable(m_ItemSlotPCView.OnDoubleClickAsObservable.Subscribe(OnDoubleClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(OnEndDrag));
	}

	private void OnClick()
	{
		switch (UsableSource)
		{
		case UsableSourceType.Vendor:
			base.ViewModel.VendorTryMove(split: false);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case UsableSourceType.Inventory:
			break;
		}
	}

	private void OnDoubleClick()
	{
		switch (UsableSource)
		{
		case UsableSourceType.Inventory:
			TryUnequip();
			break;
		case UsableSourceType.Vendor:
			base.ViewModel.VendorTryMove(split: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	private void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
	}
}
