using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationColonyProjectsBuiltListWrapperBaseView<TColonyProjectsBuiltListView> : ExplorationComponentWrapperBaseView<ExplorationColonyProjectsBuiltListWrapperVM> where TColonyProjectsBuiltListView : ColonyProjectsBuiltListBaseView
{
	[SerializeField]
	private TColonyProjectsBuiltListView m_ColonyProjectsBuiltListView;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyProjectsBuiltListView.Bind(base.ViewModel.ColonyProjectsBuiltListVM);
	}

	public List<IFloatConsoleNavigationEntity> GetFloatNavigationEntities()
	{
		if (base.ViewModel.ActiveOnScreen.Value)
		{
			return m_ColonyProjectsBuiltListView.GetNavigationEntities();
		}
		return null;
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}
}
