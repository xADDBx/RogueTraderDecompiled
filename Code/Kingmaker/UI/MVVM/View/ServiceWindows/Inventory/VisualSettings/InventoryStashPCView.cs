using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;

public class InventoryStashPCView : InventoryStashView
{
	[SerializeField]
	private InventoryDropZonePCView m_DropZonePCView;

	[SerializeField]
	private OwlcatMultiButton m_SortButton;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DropZonePCView.Or(null)?.Bind(base.ViewModel.DropZoneVM);
		AddDisposable(m_SortButton.OnLeftClickAsObservable().Subscribe(delegate
		{
			base.ViewModel.ItemSlotsGroup.SortItems();
		}));
	}
}
