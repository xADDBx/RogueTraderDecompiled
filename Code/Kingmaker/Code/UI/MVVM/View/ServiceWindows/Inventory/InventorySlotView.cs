using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;

public abstract class InventorySlotView : ItemSlotView<ItemSlotVM>
{
	[SerializeField]
	protected GameObject m_PossibleTargetHighlight;

	[Header("UsableStacks")]
	[SerializeField]
	private GameObject m_UsableStacksContainer;

	[SerializeField]
	private TextMeshProUGUI m_UsableStacksCount;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		if (m_PossibleTargetHighlight != null)
		{
			AddDisposable(base.ViewModel.PossibleTarget.Subscribe(m_PossibleTargetHighlight.SetActive));
		}
		AddDisposable(base.ViewModel.NeedBlink.Subscribe(Blink));
		AddDisposable(base.ViewModel.UsableCount.Subscribe(delegate(int value)
		{
			m_UsableStacksContainer.Or(null)?.SetActive(value > 0);
			if ((bool)m_UsableStacksCount)
			{
				m_UsableStacksCount.text = value.ToString();
			}
		}));
	}

	protected virtual void OnClick()
	{
		ServiceWindowsType currentServiceWindow = RootUIContext.Instance.CurrentServiceWindow;
		if (currentServiceWindow != ServiceWindowsType.Inventory && currentServiceWindow != ServiceWindowsType.ShipCustomization)
		{
			if (base.ViewModel.CanTransferToCargo && base.ViewModel.IsInStash)
			{
				MoveToCargo(immediately: false);
			}
			else if (base.ViewModel.CanTransferToInventory && !base.ViewModel.IsInStash)
			{
				MoveToInventory(immediately: false);
			}
		}
	}

	protected void Blink()
	{
		if ((bool)m_BlinkMark)
		{
			UISounds.Instance.Sounds.Systems.BlinkAttentionMark.Play();
			m_BlinkMark.alpha = 1f;
			m_BlinkMark.DOFade(0f, 0.65f).SetLoops(2).SetEase(Ease.OutSine)
				.SetUpdate(isIndependentUpdate: true);
		}
	}

	protected void OnDoubleClick()
	{
		EquipItem();
		DelayedInvoker.InvokeInFrames(OnHoverStart, 1);
	}

	protected void OnHoverStart()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotHoverHandler h)
		{
			h.HandleHoverStart(base.Item);
		});
	}

	protected void OnHoverEnd()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotHoverHandler h)
		{
			h.HandleHoverStop();
		});
	}

	protected void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	protected void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
	}

	protected void EquipItem()
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryEquip(base.ViewModel);
		});
	}

	protected void MoveToInventory(bool immediately)
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToInventory(base.ViewModel, immediately);
		});
	}

	protected void MoveToCargo(bool immediately)
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToCargo(base.ViewModel, immediately);
		});
	}

	protected void DropItem()
	{
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryDrop(base.ViewModel);
		});
	}

	protected void Split()
	{
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTrySplitSlot(base.ViewModel);
		});
	}
}
