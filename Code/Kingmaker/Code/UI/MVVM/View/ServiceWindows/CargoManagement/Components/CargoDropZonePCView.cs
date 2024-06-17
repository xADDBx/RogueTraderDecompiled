using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Owlcat.Runtime.UI.Controls.Selectable;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;

public class CargoDropZonePCView : ViewBase<CargoDropZoneVM>, IItemDropZone, IInventoryHandler, ISubscriber
{
	[SerializeField]
	private bool m_Interactable = true;

	[SerializeField]
	private OwlcatMultiSelectable m_DropZone;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	public BoolReactiveProperty HasItem = new BoolReactiveProperty();

	public bool Interactable => m_Interactable;

	protected override void BindViewImplementation()
	{
		base.gameObject.SetActive(value: true);
		AddDisposable(base.ViewModel.HasDropItem.Subscribe(delegate(bool value)
		{
			m_FadeAnimator.PlayAnimation(value);
		}));
		if (m_DropZone != null)
		{
			AddDisposable(base.ViewModel.CanDropItem.Subscribe(delegate(bool value)
			{
				m_DropZone.SetActiveLayer((!value) ? 1 : 0);
			}));
		}
		AddDisposable(base.ViewModel.HasDropItem.Subscribe(delegate(bool value)
		{
			HasItem.Value = value;
		}));
		if (m_Interactable)
		{
			AddDisposable(EventBus.Subscribe(this));
			AddDisposable(this.OnDropAsObservable().Subscribe(OnDrop));
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}

	public void TryDropItem(ItemSlotVM itemVM)
	{
		base.ViewModel.TryDropItem(itemVM);
	}

	void IInventoryHandler.Refresh()
	{
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryMoveToCargo(ItemSlotVM slot, bool immediately)
	{
		if (!RootUIContext.Instance.IsLootShow)
		{
			TryDropItem(slot);
		}
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
	}
}
