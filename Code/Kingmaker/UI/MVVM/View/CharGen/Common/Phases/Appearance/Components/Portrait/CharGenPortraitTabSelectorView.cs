using System.Linq;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.SelectionGroup;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Appearance.Components.Portrait;

public class CharGenPortraitTabSelectorView : ViewBase<SelectionGroupRadioVM<CharGenPortraitTabVM>>
{
	[SerializeField]
	private WidgetListMVVM m_WidgetListMvvm;

	[SerializeField]
	private CharGenPortraitTabView m_Prefab;

	private bool m_IsInit;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public void Initialize()
	{
		if (!m_IsInit)
		{
			m_IsInit = true;
		}
	}

	protected override void BindViewImplementation()
	{
		DrawEntities();
	}

	protected override void DestroyViewImplementation()
	{
	}

	private void DrawEntities()
	{
		AddDisposable(m_WidgetListMvvm.DrawEntries(base.ViewModel.EntitiesCollection, m_Prefab));
	}

	public GridConsoleNavigationBehaviour GetNavigation(IConsoleNavigationOwner owner = null)
	{
		if (m_NavigationBehaviour == null)
		{
			m_NavigationBehaviour = new GridConsoleNavigationBehaviour(owner);
		}
		m_NavigationBehaviour.SetEntitiesGrid(m_WidgetListMvvm.Entries.Cast<IConsoleEntity>().ToList(), 2);
		return m_NavigationBehaviour;
	}
}
