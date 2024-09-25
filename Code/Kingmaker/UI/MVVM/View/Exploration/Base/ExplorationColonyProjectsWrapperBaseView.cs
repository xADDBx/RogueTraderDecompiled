using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationColonyProjectsWrapperBaseView<TColonyProjectsView> : ExplorationComponentWrapperBaseView<ExplorationColonyProjectsWrapperVM> where TColonyProjectsView : ColonyProjectsBaseView
{
	[SerializeField]
	private TColonyProjectsView m_ColonyProjectsView;

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyProjectsView.Bind(base.ViewModel.ColonyProjectsVM);
	}

	public void Initialize()
	{
		m_ColonyProjectsView.Initialize();
	}
}
