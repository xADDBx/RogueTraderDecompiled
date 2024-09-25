using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Owlcat.Runtime.UniRx;

namespace Kingmaker.Code.UI.MVVM.View.Vendor;

public class VendorSlotPCView : VendorSlotView
{
	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotView.Bind(base.ViewModel);
		if (m_ItemSlotView is ItemSlotPCView itemSlotPCView)
		{
			AddDisposable(itemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(base.OnClick));
			AddDisposable(itemSlotPCView.OnDoubleClickAsObservable.Subscribe(base.OnDoubleClick));
			ItemSlotPCView itemSlotPCView2 = m_ItemSlotView as ItemSlotPCView;
			if ((bool)itemSlotPCView2)
			{
				AddDisposable(itemSlotPCView2.OnEndDragCommand.Subscribe(DelUpd));
			}
			RefreshItem();
		}
	}

	private void DelUpd()
	{
		DelayedInvoker.InvokeInFrames(delegate
		{
			RefreshItem();
		}, 1);
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		List<ContextMenuCollectionEntity> value = new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.Buy, base.ViewModel.VendorTryBuyAll, base.ViewModel.HasItem),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		};
		base.ViewModel.ContextMenu.Value = value;
	}
}
