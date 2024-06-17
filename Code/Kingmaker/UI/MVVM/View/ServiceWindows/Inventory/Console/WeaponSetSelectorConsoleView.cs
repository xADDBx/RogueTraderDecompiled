using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory.Console;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.Inventory.Console;

public class WeaponSetSelectorConsoleView : WeaponSetSelectorPCView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private Coroutine m_AddHintsCo;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		if (m_AddHintsCo != null)
		{
			StopCoroutine(m_AddHintsCo);
		}
		base.DestroyViewImplementation();
	}

	private void CreateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		foreach (WeaponSetBaseView weaponSetView in m_WeaponSetViews)
		{
			if (weaponSetView is WeaponSetConsoleView weaponSetConsoleView)
			{
				m_NavigationBehaviour.AddRow(weaponSetConsoleView.GetNavigationEntities());
			}
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation();
		}
		return m_NavigationBehaviour?.Entities.ToList();
	}
}
