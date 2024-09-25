using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.MVVM.VM.InfoWindow;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SelectorWindow;

public class SelectorWindowVM<TEntityVM> : SelectionGroupRadioVM<TEntityVM> where TEntityVM : SelectionGroupEntityVM
{
	public readonly InfoSectionVM InfoSectionVM;

	public TEntityVM CurrentSelected;

	private readonly Action<TEntityVM> m_OnConfirm;

	private readonly Action m_OnDecline;

	public readonly EquipSlotVM Slot;

	public SelectorWindowVM(Action<TEntityVM> onConfirm, Action onDecline, ReactiveCollection<TEntityVM> entitiesCollection, ReactiveProperty<TEntityVM> entity = null, EquipSlotVM equippedSlot = null)
		: base(entitiesCollection, entity, cyclical: false)
	{
		m_OnConfirm = onConfirm;
		m_OnDecline = onDecline;
		Slot = equippedSlot;
		AddDisposable(InfoSectionVM = new InfoSectionVM());
	}

	public SelectorWindowVM(Action<TEntityVM> onConfirm, Action onDecline, List<TEntityVM> visibleCollection, ReactiveProperty<TEntityVM> entity = null, EquipSlotVM equippedSlot = null)
		: base(visibleCollection, entity, cyclical: false)
	{
		m_OnConfirm = onConfirm;
		m_OnDecline = onDecline;
		Slot = equippedSlot;
		AddDisposable(InfoSectionVM = new InfoSectionVM());
	}

	public void Confirm(TEntityVM entityVM)
	{
		m_OnConfirm?.Invoke(entityVM);
	}

	public void Back()
	{
		m_OnDecline?.Invoke();
	}

	public void SetCurrentSelected(TEntityVM entity)
	{
		CurrentSelected = entity;
	}
}
