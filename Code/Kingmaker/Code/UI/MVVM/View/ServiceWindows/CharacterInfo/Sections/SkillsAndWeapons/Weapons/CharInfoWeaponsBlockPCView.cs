using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.SkillsAndWeapons.Weapons;

public class CharInfoWeaponsBlockPCView : CharInfoComponentView<CharInfoWeaponsBlockVM>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoWeaponSetPCView m_WeaponSetViewPrefab;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public List<CharInfoWeaponSetPCView> Entries => m_WidgetList.Entries?.Select((IWidgetView e) => e as CharInfoWeaponSetPCView).ToList() ?? new List<CharInfoWeaponSetPCView>();

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		DrawEntities();
		UpdateNavigation();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_WidgetList.Clear();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		DrawEntities();
		UpdateNavigation();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.WeaponSets.ToArray(), m_WeaponSetViewPrefab);
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour == null)
		{
			AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		}
		else
		{
			m_NavigationBehaviour.Clear();
		}
		foreach (CharInfoWeaponSetPCView entry in Entries)
		{
			m_NavigationBehaviour.AddRow<GridConsoleNavigationBehaviour>(entry.GetNavigation());
		}
	}

	public GridConsoleNavigationBehaviour GetNavigation()
	{
		return m_NavigationBehaviour;
	}
}
