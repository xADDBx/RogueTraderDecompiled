using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class WeaponSetConsoleView : WeaponSetBaseView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigationIfNeeded();
		SetChangeSlotActions(m_PrimaryHand);
		SetChangeSlotActions(m_SecondaryHand);
	}

	private void CreateNavigationIfNeeded()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
			m_NavigationBehaviour.AddRow<InventoryEquipSlotConsoleView>(m_PrimaryHand as InventoryEquipSlotConsoleView);
			m_NavigationBehaviour.AddRow<InventoryEquipSlotConsoleView>(m_SecondaryHand as InventoryEquipSlotConsoleView);
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		CreateNavigationIfNeeded();
		return m_NavigationBehaviour?.Entities.ToList();
	}

	private void SetChangeSlotActions(InventoryEquipSlotView slot)
	{
		if (slot is InventoryWeaponSlotConsoleView inventoryWeaponSlotConsoleView)
		{
			inventoryWeaponSlotConsoleView.SetOnWeaponSetChange(delegate
			{
				base.ViewModel.SetSelected(state: true);
			}, delegate
			{
				WeaponSetVM viewModel = base.ViewModel;
				return viewModel != null && !viewModel.IsSelected.Value;
			});
		}
	}
}
