using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.PubSubSystem.Core;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.PC;

public class InsertableLootSlotPCView : InsertableLootSlotView
{
	[Header("PC")]
	[SerializeField]
	private ItemSlotPCView m_ItemSlotPCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ItemSlotPCView.Bind(base.ViewModel);
		AddDisposable(m_ItemSlotPCView.OnSingleLeftClickAsObservable.Subscribe(base.OnClick));
		AddDisposable(m_ItemSlotPCView.OnBeginDragCommand.Subscribe(OnBeginDrag));
		AddDisposable(m_ItemSlotPCView.OnEndDragCommand.Subscribe(OnEndDrag));
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
}
