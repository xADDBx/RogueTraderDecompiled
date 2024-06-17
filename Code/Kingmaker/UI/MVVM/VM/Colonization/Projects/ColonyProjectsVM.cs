using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.Colonization;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Sound;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsVM : ColonyUIComponentVM, IColonizationProjectsUIHandler, ISubscriber, IColonizationProjectsHandler
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> StartAvailable = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> ShowBlockedProjects = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> ShowFinishedProjects = new ReactiveProperty<bool>(initialValue: false);

	public readonly ColonyProjectsPageVM ColonyProjectPageVM;

	public readonly ColonyProjectsNavigationVM NavigationVM;

	private IDisposable m_EventBusDispose;

	public bool IsColonyManagement => m_IsColonyManagement;

	public ColonyProjectsVM()
	{
		bool isShowBlockedColonyProjects = Game.Instance.Player.IsShowBlockedColonyProjects;
		bool isShowFinishedColonyProjects = Game.Instance.Player.IsShowFinishedColonyProjects;
		AddDisposable(ColonyProjectPageVM = new ColonyProjectsPageVM());
		AddDisposable(NavigationVM = new ColonyProjectsNavigationVM(isShowBlockedColonyProjects, isShowFinishedColonyProjects));
		ShowBlockedProjects.Value = isShowBlockedColonyProjects;
		ShowFinishedProjects.Value = isShowFinishedColonyProjects;
	}

	protected override void DisposeImplementation()
	{
		m_EventBusDispose?.Dispose();
		m_EventBusDispose = null;
		NavigationVM.Clear();
	}

	protected override void SetColonyImpl(Colony colony)
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
		ColonyProjectPageVM.SetColony(colony);
		NavigationVM.SetColony(colony);
	}

	public void StartProject()
	{
		if (m_Colony == null)
		{
			PFLog.System.Error("ColonyProjectsVM.StartProject - no colony!");
			return;
		}
		UISounds.Instance.Sounds.SpaceColonization.StartProject.Play();
		Game.Instance.GameCommandQueue.StartColonyProject(m_Colony.Blueprint.ToReference<BlueprintColonyReference>(), ColonyProjectPageVM.BlueprintColonyProject.ToReference<BlueprintColonyProjectReference>());
		UpdateStartAvailability(ColonyProjectPageVM.BlueprintColonyProject);
	}

	public void SwitchBlockedProjects()
	{
		ShowBlockedProjects.Value = !ShowBlockedProjects.Value;
		NavigationVM.ShowBlockedProjects(ShowBlockedProjects.Value);
		Game.Instance.Player.IsShowBlockedColonyProjects = ShowBlockedProjects.Value;
	}

	public void SwitchFinishedProjects()
	{
		ShowFinishedProjects.Value = !ShowFinishedProjects.Value;
		NavigationVM.ShowFinishedProjects(ShowFinishedProjects.Value);
		Game.Instance.Player.IsShowFinishedColonyProjects = ShowFinishedProjects.Value;
	}

	public void HandleColonyProjectsUIOpen(Colony colony)
	{
		ShouldShow.Value = true;
		List<BlueprintColonyProject> list = colony.Blueprint.Projects.Dereference().ToList();
		UpdateStartAvailability(list[0]);
	}

	public void HandleColonyProjectsUIClose()
	{
		ShouldShow.Value = false;
	}

	public void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject)
	{
		UpdateStartAvailability(blueprintColonyProject);
	}

	public void ClearNavigation()
	{
		NavigationVM.Clear();
	}

	private void UpdateStartAvailability(BlueprintColonyProject blueprintColonyProject)
	{
		if (m_Colony == null)
		{
			PFLog.System.Error("ColonyProjectsVM.UpdateStartAvailability - no colony!");
			return;
		}
		StartAvailable.Value = m_Colony.ProjectCanStart(blueprintColonyProject) && m_Colony.Projects.All((ColonyProject p) => p.Blueprint != blueprintColonyProject);
	}

	public void HandleColonyProjectStarted(Colony colony, ColonyProject project)
	{
		if (ShouldShow.Value && ColonyProjectPageVM.BlueprintColonyProject == project.Blueprint)
		{
			UpdateStartAvailability(project.Blueprint);
		}
	}

	public void HandleColonyProjectFinished(Colony colony, ColonyProject project)
	{
		if (ShouldShow.Value && ColonyProjectPageVM.BlueprintColonyProject == project.Blueprint)
		{
			UpdateStartAvailability(project.Blueprint);
		}
	}
}
