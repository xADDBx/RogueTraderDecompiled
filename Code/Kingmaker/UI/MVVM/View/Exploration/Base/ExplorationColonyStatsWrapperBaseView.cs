using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public abstract class ExplorationColonyStatsWrapperBaseView<TColonyStatsView> : ExplorationComponentWrapperBaseView<ExplorationColonyStatsWrapperVM> where TColonyStatsView : ColonyStatsBaseView
{
	[SerializeField]
	private TColonyStatsView m_ColonyStatsView;

	public void Initialize()
	{
		m_ColonyStatsView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyStatsView.Bind(base.ViewModel.ColonyStatsVM);
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		if (base.ViewModel.ActiveOnScreen.Value)
		{
			return m_ColonyStatsView.GetNavigationEntities();
		}
		return null;
	}
}
