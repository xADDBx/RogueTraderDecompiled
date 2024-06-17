using System;
using System.Collections.Generic;
using DG.Tweening;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu.Utils;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.DragNDrop;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.Tooltips;
using Owlcat.Runtime.UniRx;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Slots;

public class ItemSlotPCView : ItemSlotBaseView, IDraggableElement
{
	[SerializeField]
	protected CanvasGroup m_FadeContainer;

	private bool m_BeginDrag;

	[HideInInspector]
	public bool IsDraggable = true;

	public readonly ReactiveCommand OnBeginDragCommand = new ReactiveCommand();

	public readonly ReactiveCommand OnEndDragCommand = new ReactiveCommand();

	public IObservable<Unit> OnSingleLeftClickAsObservable => from _ in m_MainButton.OnSingleLeftClickAsObservable()
		where !m_BeginDrag
		select _;

	public IObservable<Unit> OnDoubleClickAsObservable => m_MainButton.OnLeftDoubleClickAsObservable();

	public IObservable<Unit> OnRightClickAsObservable => m_MainButton.OnRightClickAsObservable();

	public IObservable<Unit> OnLeftClickAsObservable => from _ in m_MainButton.OnLeftClickAsObservable()
		where !m_BeginDrag
		select _;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.None, InfoCallConsoleMethod.None, isGlossary: false, isEncyclopedia: false, GetParentContainer(), 0, 0, 0, new List<Vector2>
		{
			new Vector2(0f, 0.5f),
			new Vector2(1f, 0.5f)
		})));
		AddDisposable(this.SetContextMenu(base.ViewModel.ContextMenu));
		m_BeginDrag = false;
		SubscribeInteractions();
		UpdateSlotLayer();
	}

	protected override void DestroyViewImplementation()
	{
		if (m_BeginDrag)
		{
			DragNDropManager.Instance.Or(null)?.CancelDrag();
			m_BeginDrag = false;
		}
	}

	private void SubscribeInteractions()
	{
		AddDisposable(OnSingleLeftClickAsObservable.Subscribe(OnClick));
		AddDisposable(OnDoubleClickAsObservable.Subscribe(OnDoubleClick));
		if (IsDraggable)
		{
			AddDisposable(this.OnBeginDragAsObservable().Subscribe(OnBeginDrag));
			AddDisposable(this.OnDragAsObservable().Subscribe(OnDrag));
			AddDisposable(this.OnEndDragAsObservable().Subscribe(OnEndDrag));
			AddDisposable(this.OnDropAsObservable().Subscribe(OnDrop));
		}
	}

	public void SetMainButtonHoverSound(UISounds.ButtonSoundsEnum soundType)
	{
		UISounds.Instance.SetHoverSound(m_MainButton, soundType);
	}

	private new void UpdateSlotLayer()
	{
		if (m_MainButton.MultiLayerNames != null)
		{
			m_MainButton.SetActiveLayer((base.ViewModel.Count.Value > 0) ? 1 : 0);
		}
		else
		{
			m_MainButton.SetActiveLayer(0);
		}
	}

	private void OnClick()
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(null);
		});
	}

	private void OnDoubleClick()
	{
	}

	private void OnBeginDrag(PointerEventData eventData)
	{
		m_BeginDrag = true;
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnBeginDrag(eventData, base.gameObject);
		});
		OnBeginDragCommand.Execute();
		UpdateSlotLayer();
	}

	private void OnDrag(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrag(eventData);
		});
	}

	private void OnEndDrag(PointerEventData eventData)
	{
		m_BeginDrag = false;
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnEndDrag(eventData);
		});
		OnEndDragCommand.Execute();
		UpdateSlotLayer();
	}

	private void OnDrop(PointerEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IDragDropEventUIHandler h)
		{
			h.OnDrop(eventData, base.gameObject);
		});
	}

	public void StartDrag()
	{
		m_FadeContainer.DOFade(0.5f, 0.1f).SetUpdate(isIndependentUpdate: true);
		UISounds.Instance.PlayItemSound(SlotAction.Take, base.ViewModel.Item.Value, equipSound: false);
	}

	public void EndDrag(PointerEventData eventData)
	{
		m_FadeContainer.DOFade(1f, 0.1f).SetUpdate(isIndependentUpdate: true);
		GameObject dropTarget = DragNDropManager.DropTarget;
		if (dropTarget == null)
		{
			UISounds.Instance.Sounds.Inventory.ErrorEquip.Play();
			return;
		}
		IItemDropZone itemDropZone = dropTarget.GetComponent<IItemDropZone>() ?? dropTarget.GetComponentInParent<IItemDropZone>();
		ItemSlotVM targetSlot = dropTarget.GetComponent<IItemSlotView>()?.SlotVM ?? dropTarget.GetComponentInParent<IItemSlotView>()?.SlotVM;
		if (itemDropZone != null && itemDropZone.Interactable)
		{
			itemDropZone.TryDropItem(base.ViewModel);
		}
		else
		{
			EventBus.RaiseEvent(delegate(INewSlotsHandler h)
			{
				h.HandleTryMoveSlot(base.ViewModel, targetSlot);
			});
		}
		bool isNotable = base.ViewModel.Item.Value.Blueprint.IsNotable;
		if (UIUtilityItem.GetEquipPosibility(base.ViewModel.Item.Value)[0] || isNotable || base.ViewModel.Item.Value is ItemEntitySimple || !(targetSlot is EquipSlotVM))
		{
			UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.Item.Value, targetSlot is EquipSlotVM);
		}
		else
		{
			UISounds.Instance.Sounds.Combat.CombatGridCantPerformActionClick.Play();
		}
	}

	public bool SetDragSlot(DragNDropManager slot)
	{
		if (!base.ViewModel.HasItem)
		{
			return false;
		}
		Image icon = slot.Icon;
		ItemEntity value = base.ViewModel.Item.Value;
		icon.sprite = ((value != null) ? value.Icon.Or(Game.Instance.BlueprintRoot.UIConfig.UIIcons.DefaultItemIcon) : null);
		TextMeshProUGUI count = slot.Count;
		ItemEntity value2 = base.ViewModel.Item.Value;
		count.text = ((value2 == null || !value2.IsStackable) ? string.Empty : base.ViewModel.Item.Value?.Count.ToString());
		slot.OverideSize = new Vector2(96f, 96f);
		return true;
	}

	public void CancelDrag()
	{
		m_BeginDrag = false;
	}
}
