using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common.CareerPathProgression.Items;
using Owlcat.Runtime.UI.ConsoleTools;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class InventoryWeaponSlotConsoleView : InventoryEquipSlotConsoleView, IFuncAdditionalClickHandler, IConsoleEntity
{
	private Action m_OnWeaponSetChange;

	private Func<bool> m_CanChange;

	public void SetOnWeaponSetChange(Action onChange, Func<bool> canChange)
	{
		m_OnWeaponSetChange = onChange;
		m_CanChange = canChange;
	}

	public bool CanFuncAdditionalClick()
	{
		if (m_CanChange != null)
		{
			return m_CanChange();
		}
		return false;
	}

	public void OnFuncAdditionalClick()
	{
		m_OnWeaponSetChange?.Invoke();
	}

	public string GetFuncAdditionalClickHint()
	{
		return UIStrings.Instance.InventoryScreen.ChangeWeaponSet.Text;
	}
}
