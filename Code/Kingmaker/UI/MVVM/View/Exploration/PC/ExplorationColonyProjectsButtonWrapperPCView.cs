using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.PC;
using Kingmaker.UI.MVVM.View.Exploration.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.PC;

public class ExplorationColonyProjectsButtonWrapperPCView : ExplorationComponentWrapperBaseView<ExplorationColonyProjectsButtonWrapperVM>
{
	[SerializeField]
	private ColonyProjectsButtonPCView m_ColonyProjectsButtonPCView;

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyProjectsButtonPCView.Bind(base.ViewModel.ColonyProjectsButtonVM);
	}

	public void Initialize()
	{
		m_ColonyProjectsButtonPCView.Initialize();
	}
}
