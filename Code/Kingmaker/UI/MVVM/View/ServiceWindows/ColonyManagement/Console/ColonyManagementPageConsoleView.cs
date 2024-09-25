using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Kingmaker.UI.MVVM.View.Colonization.Console;
using Kingmaker.UI.MVVM.View.Exploration.Console;
using Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Base;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.ColonyManagement.Console;

public class ColonyManagementPageConsoleView : ColonyManagementPageBaseView
{
	[Header("General")]
	[SerializeField]
	private ExplorationSpaceResourcesConsoleView m_ExplorationSpaceResourcesConsoleView;

	[SerializeField]
	private ColonyStatsConsoleView m_ColonyStatsConsoleView;

	[SerializeField]
	private ColonyTraitsConsoleView m_ColonyTraitsConsoleView;

	[SerializeField]
	private ColonyEventsConsoleView m_ColonyEventsConsoleView;

	[SerializeField]
	private ColonyRewardsConsoleView m_ColonyRewardsConsoleView;

	[Header("Projects")]
	[SerializeField]
	private ColonyProjectsConsoleView m_ColonyProjectsConsoleView;

	[SerializeField]
	private ColonyProjectsBuiltListConsoleView m_ColonyProjectsBuiltListConsoleView;

	protected override void InitializeImpl()
	{
		m_ColonyTraitsConsoleView.Initialize();
		m_ColonyEventsConsoleView.Initialize();
		m_ColonyRewardsConsoleView.Initialize();
		m_ColonyProjectsConsoleView.Initialize();
		m_ColonyStatsConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ExplorationSpaceResourcesConsoleView.Bind(base.ViewModel.ExplorationSpaceResourcesVM);
		m_ColonyStatsConsoleView.Bind(base.ViewModel.ColonyStatsVM);
		m_ColonyTraitsConsoleView.Bind(base.ViewModel.ColonyTraitsVM);
		m_ColonyEventsConsoleView.Bind(base.ViewModel.ColonyEventsVM);
		m_ColonyRewardsConsoleView.Bind(base.ViewModel.ColonyRewardsVM);
		m_ColonyProjectsConsoleView.Bind(base.ViewModel.ColonyProjectsVM);
		m_ColonyProjectsBuiltListConsoleView.Bind(base.ViewModel.ColonyProjectsBuiltListVM);
	}

	public IEnumerable<IConsoleNavigationEntity> GetEventsNavigationEntities()
	{
		return m_ColonyEventsConsoleView.GetNavigationEntities();
	}

	public IEnumerable<IConsoleNavigationEntity> GetStatsNavigationEntities()
	{
		return m_ColonyStatsConsoleView.GetNavigationEntities();
	}

	public IEnumerable<IConsoleNavigationEntity> GetTraitsNavigationEntities()
	{
		return m_ColonyTraitsConsoleView.GetNavigationEntities();
	}

	public List<IFloatConsoleNavigationEntity> GetProjectsBuiltListFloatNavigationEntities()
	{
		return m_ColonyProjectsBuiltListConsoleView.GetNavigationEntities();
	}

	public IEnumerable<IConsoleNavigationEntity> GetSpaceResourcesNavigationEntities()
	{
		return m_ExplorationSpaceResourcesConsoleView.GetNavigationEntities();
	}

	public void ScrollEventsList(ColonyEventBaseView entity)
	{
		m_ColonyEventsConsoleView.ScrollList(entity);
	}

	public void ScrollTraitsList(ColonyTraitBaseView entity)
	{
		m_ColonyTraitsConsoleView.ScrollList(entity);
	}
}
