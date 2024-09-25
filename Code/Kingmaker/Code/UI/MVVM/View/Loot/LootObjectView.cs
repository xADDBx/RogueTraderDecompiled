using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Loot;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.MVVM;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public class LootObjectView : VirtualListElementViewBase<LootObjectVM>, IConsoleEntity
{
	[SerializeField]
	private ItemSlotsGroupView m_SlotsGroup;

	[SerializeField]
	private LootSlotView m_SlotPrefab;

	[SerializeField]
	private InventoryDropZonePCView m_InventoryDropZonePCView;

	[SerializeField]
	private CargoDropZonePCView m_CargoDropZonePCView;

	private bool m_Init;

	public ItemSlotsGroupView SlotsGroup => m_SlotsGroup;

	public void Initialize()
	{
		if (!m_Init)
		{
			m_SlotsGroup.Initialize(m_SlotPrefab);
			m_Init = true;
		}
	}

	protected override void BindViewImplementation()
	{
		Initialize();
		SetupSlots();
		m_InventoryDropZonePCView.Or(null)?.Bind(base.ViewModel.InventoryDropZoneVM);
		m_CargoDropZonePCView.Or(null)?.Bind(base.ViewModel.CargoDropZoneVM);
	}

	private void SetupSlots()
	{
		m_SlotsGroup.Bind(base.ViewModel.SlotsGroup);
	}

	protected override void DestroyViewImplementation()
	{
	}
}
