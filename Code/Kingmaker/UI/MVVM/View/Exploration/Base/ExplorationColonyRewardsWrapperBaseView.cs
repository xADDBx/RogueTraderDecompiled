using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.VM.Exploration;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Exploration.Base;

public class ExplorationColonyRewardsWrapperBaseView<TColonyRewardsView> : ExplorationComponentWrapperBaseView<ExplorationColonyRewardsWrapperVM> where TColonyRewardsView : ColonyRewardsBaseView
{
	[SerializeField]
	private TColonyRewardsView m_ColonyRewardsView;

	public void Initialize()
	{
		m_ColonyRewardsView.Initialize();
	}

	public override IEnumerable<IConsoleNavigationEntity> GetNavigationEntities()
	{
		return null;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ColonyRewardsView.Bind(base.ViewModel.ColonyRewardsVM);
	}
}
