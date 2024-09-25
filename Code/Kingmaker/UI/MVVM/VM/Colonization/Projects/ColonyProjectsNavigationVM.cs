using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsNavigationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IColonizationProjectsUIHandler, ISubscriber, IColonizationProjectsHandler
{
	public readonly AutoDisposingList<ColonyProjectVM> NavigationElements = new AutoDisposingList<ColonyProjectVM>();

	public readonly AutoDisposingList<ColonyProjectRankVM> Ranks = new AutoDisposingList<ColonyProjectRankVM>();

	public readonly ReactiveCommand ProjectsCreated = new ReactiveCommand();

	public readonly ReactiveCommand ProjectsUpdated = new ReactiveCommand();

	private readonly Dictionary<BlueprintColonyProject, ColonyProjectVM> m_ProjectsNavElements = new Dictionary<BlueprintColonyProject, ColonyProjectVM>();

	private bool m_IsStartingProjects;

	private bool m_ShowBlockedProjects;

	private bool m_ShowFinishedProjects;

	private Colony m_Colony;

	private IDisposable m_EventBusDispose;

	public ColonyProjectsNavigationVM(bool showBlockedProjects, bool showFinishedProjects)
	{
		m_ShowBlockedProjects = showBlockedProjects;
		m_ShowFinishedProjects = showFinishedProjects;
		ColonyProjectRank[] array = (ColonyProjectRank[])Enum.GetValues(typeof(ColonyProjectRank));
		foreach (ColonyProjectRank rank in array)
		{
			Ranks.Add(new ColonyProjectRankVM(rank));
		}
	}

	protected override void DisposeImplementation()
	{
		m_EventBusDispose?.Dispose();
		m_EventBusDispose = null;
		m_Colony = null;
		Clear();
	}

	public void SetColony(Colony colony)
	{
		if (colony != null)
		{
			m_EventBusDispose = EventBus.Subscribe(this);
		}
		else
		{
			m_EventBusDispose?.Dispose();
			m_EventBusDispose = null;
		}
		m_Colony = colony;
	}

	public void Clear()
	{
		NavigationElements.Clear();
		Ranks.Clear();
		m_ProjectsNavElements.Clear();
	}

	public void ShowBlockedProjects(bool show)
	{
		m_ShowBlockedProjects = show;
		UpdateProjects();
	}

	public void ShowFinishedProjects(bool show)
	{
		m_ShowFinishedProjects = show;
		UpdateProjects();
	}

	public void HandleColonyProjectsUIOpen(Colony colony)
	{
		UpdateStartingProjectsState();
		List<BlueprintColonyProject> projects = colony.Blueprint.Projects.Dereference().ToList();
		SetProjects(projects);
		SelectFirstVisibleElement();
	}

	public void HandleColonyProjectsUIClose()
	{
		Clear();
	}

	public void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject)
	{
		SetSelection(blueprintColonyProject);
	}

	public void HandleColonyProjectStarted(Colony colony, ColonyProject project)
	{
		if (m_Colony != colony)
		{
			return;
		}
		UpdateStartingProjectsState();
		BlueprintColonyProject selection = project.Blueprint;
		List<BlueprintColonyProject> list = colony.Blueprint.Projects.Dereference().ToList();
		foreach (ColonyProjectVM value in m_ProjectsNavElements.Values)
		{
			if (value != null && value.IsSelected.Value)
			{
				if (list.Contains(value.BlueprintColonyProject))
				{
					selection = value.BlueprintColonyProject;
				}
				break;
			}
		}
		SetProjects(list);
		SetSelection(selection);
	}

	public void HandleColonyProjectFinished(Colony colony, ColonyProject project)
	{
		UpdateProjects();
	}

	private void SetProjects(List<BlueprintColonyProject> projects)
	{
		if (m_Colony == null)
		{
			PFLog.UI.Error("ColonyProjectsNavigationVM.SetProjects - colony is null!");
			return;
		}
		Clear();
		foreach (BlueprintColonyProject project in projects)
		{
			if (project != null && m_Colony.ProjectIsAvailable(project))
			{
				ColonyProjectVM colonyProjectVM = new ColonyProjectVM(project, m_Colony);
				AddDisposable(colonyProjectVM);
				NavigationElements.Add(colonyProjectVM);
				m_ProjectsNavElements[project] = colonyProjectVM;
			}
		}
		UpdateProjects();
		ProjectsCreated.Execute();
	}

	private void UpdateProjects()
	{
		bool flag = false;
		foreach (ColonyProjectVM navigationElement in NavigationElements)
		{
			navigationElement.SetShouldShow(m_IsStartingProjects, m_ShowBlockedProjects, m_ShowFinishedProjects);
			if (!flag)
			{
				flag = navigationElement.IsSelected.Value && !navigationElement.IsVisible;
			}
		}
		if (flag)
		{
			CheckSelectedProjectVisible();
		}
		ProjectsUpdated.Execute();
	}

	private void SetSelection(BlueprintColonyProject colonyProject)
	{
		foreach (ColonyProjectVM value in m_ProjectsNavElements.Values)
		{
			value.SetSelection(colonyProject);
		}
	}

	private void SelectFirstVisibleElement()
	{
		foreach (ColonyProjectVM navigationElement in NavigationElements)
		{
			if (navigationElement.IsVisible)
			{
				SetSelection(navigationElement.BlueprintColonyProject);
				navigationElement.SelectPage();
				break;
			}
		}
	}

	private void CheckSelectedProjectVisible()
	{
		ColonyProjectVM colonyProjectVM = NavigationElements.FirstOrDefault((ColonyProjectVM elem) => elem.IsVisible);
		SetSelection(colonyProjectVM?.BlueprintColonyProject);
		colonyProjectVM?.SelectPage();
	}

	private void UpdateStartingProjectsState()
	{
		if (m_Colony == null)
		{
			PFLog.UI.Error("ColonyProjectsNavigationVM.UpdateStartingProjectsState - colony is null!");
		}
		else
		{
			m_IsStartingProjects = m_Colony.Projects.Empty();
		}
	}
}
