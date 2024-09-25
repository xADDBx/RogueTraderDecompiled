using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class LootInventorySlotPCView : LootInventorySlotView
{
	[Header("PC")]
	[SerializeField]
	protected ItemSlotPCView m_ItemSlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotPCView.OnLeftClickAsObservable.Subscribe(OnClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(OnEndDrag));
	}

	protected new void OnBeginDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStart(base.Item);
		});
	}

	protected new void OnEndDrag()
	{
		EventBus.RaiseEvent(delegate(IEquipSlotPossibleTarget h)
		{
			h.HandleHighlightStop();
		});
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		base.ViewModel.ContextMenu.Value = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Split, base.Split, base.ViewModel.IsPosibleSplit),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
	}
}
