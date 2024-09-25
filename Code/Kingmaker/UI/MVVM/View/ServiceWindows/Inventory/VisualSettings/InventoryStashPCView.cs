using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.VisualSettings;

public class InventoryStashPCView : InventoryStashView
{
	[SerializeField]
	private InventoryDropZonePCView m_DropZonePCView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_DropZonePCView.Or(null)?.Bind(base.ViewModel.DropZoneVM);
	}
}
