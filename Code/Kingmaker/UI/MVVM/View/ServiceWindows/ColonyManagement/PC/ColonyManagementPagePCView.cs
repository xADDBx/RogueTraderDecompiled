using Kingmaker.UI.MVVM.View.Colonization.PC;
using Kingmaker.UI.MVVM.View.Exploration.PC;
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.PC;

public class ColonyManagementPagePCView : ColonyManagementPageBaseView
{
	[Header("General")]
	[SerializeField]
	private ExplorationSpaceResourcesPCView m_ExplorationSpaceResourcesPCView;

	[SerializeField]
	private ColonyStatsPCView m_ColonyStatsView;

	[SerializeField]
	private ColonyTraitsPCView m_ColonyTraitsPCView;

	[SerializeField]
	private ColonyEventsPCView m_ColonyEventsPCView;

	[SerializeField]
	private ColonyRewardsPCView m_ColonyRewardsPCView;

	[Header("Projects")]
	[SerializeField]
	private ColonyProjectsPCView m_ColonyProjectsView;

	[SerializeField]
	private ColonyProjectsButtonPCView m_ColonyProjectsButtonView;

	[SerializeField]
	private ColonyProjectsBuiltListPCView m_ColonyProjectsBuiltListView;

	protected override void InitializeImpl()
	{
		m_ColonyTraitsPCView.Initialize();
		m_ColonyEventsPCView.Initialize();
		m_ColonyRewardsPCView.Initialize();
		m_ColonyProjectsView.Initialize();
		m_ColonyProjectsButtonView.Initialize();
		m_ColonyStatsView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ExplorationSpaceResourcesPCView.Bind(base.ViewModel.ExplorationSpaceResourcesVM);
		m_ColonyStatsView.Bind(base.ViewModel.ColonyStatsVM);
		m_ColonyTraitsPCView.Bind(base.ViewModel.ColonyTraitsVM);
		m_ColonyEventsPCView.Bind(base.ViewModel.ColonyEventsVM);
		m_ColonyRewardsPCView.Bind(base.ViewModel.ColonyRewardsVM);
		m_ColonyProjectsView.Bind(base.ViewModel.ColonyProjectsVM);
		m_ColonyProjectsButtonView.Bind(base.ViewModel.ColonyProjectsButtonVM);
		m_ColonyProjectsBuiltListView.Bind(base.ViewModel.ColonyProjectsBuiltListVM);
	}
}
