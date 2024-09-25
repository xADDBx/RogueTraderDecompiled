using System.Collections.Generic;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationSpaceResourcesWrapperBaseView<TExplorationSpaceResourcesView> : ExplorationComponentWrapperBaseView<ExplorationSpaceResourcesWrapperVM> where TExplorationSpaceResourcesView : ExplorationSpaceResourcesBaseView
{
	[SerializeField]
	private TExplorationSpaceResourcesView m_ExplorationSpaceResourcesPCView;

	protected override void BindViewImplementation()
	{
		m_ExplorationSpaceResourcesPCView.Bind(base.ViewModel.ExplorationSpaceResourcesVM);
	}

	protected override void DestroyViewImplementation()
	{
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return m_ExplorationSpaceResourcesPCView.GetNavigationEntities();
	}
}
