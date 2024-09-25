using System;
using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> ShouldShow = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsExcluded = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsFinished = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsBuilding = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> OtherProjectIsBuilding = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> IsNotMeetRequirements = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<int> Progress = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<int> SegmentsToBuild = new ReactiveProperty<int>(0);

	public readonly ReactiveProperty<string> Title = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	private readonly BlueprintColonyProject m_BlueprintColonyProject;

	private ColonyProject m_ColonyProject;

	private readonly Colony m_Colony;

	private readonly ColonyProjectRank m_Rank;

	public BlueprintColonyProject BlueprintColonyProject => m_BlueprintColonyProject;

	public Colony Colony => m_Colony;

	public ColonyProjectRank Rank => m_Rank;

	public bool IsVisible => ShouldShow.Value;

	public ColonyProjectVM(BlueprintColonyProject blueprintColonyProject, Colony colony)
	{
		m_BlueprintColonyProject = blueprintColonyProject;
		m_Colony = colony;
		m_Rank = blueprintColonyProject.Rank;
		Title.Value = blueprintColonyProject.Name;
		Icon.Value = (blueprintColonyProject.Icon ? blueprintColonyProject.Icon : UIConfig.Instance.UIIcons.DefaultColonyProjectIcon);
		CalculateProgress();
		AddDisposable(MainThreadDispatcher.InfrequentUpdateAsObservable().Subscribe(CalculateProgress));
	}

	public void FullUpdate()
	{
		UpdateColonyProject();
		UpdateBuildingState();
		UpdateFinishedState();
		UpdateExcludedState();
		UpdateOtherProjectIsBuildingState();
		UpdateMeetRequirementsState();
	}

	public void SetShouldShow(bool isStartingProjectsState, bool showBlockedProjectsState, bool showFinishedProjectsState)
	{
		FullUpdate();
		if (isStartingProjectsState)
		{
			ShouldShow.Value = m_BlueprintColonyProject.IsStartingProject;
		}
		else if (IsExcluded.Value)
		{
			ShouldShow.Value = showBlockedProjectsState;
		}
		else if (IsFinished.Value)
		{
			ShouldShow.Value = showFinishedProjectsState;
		}
		else
		{
			ShouldShow.Value = true;
		}
	}

	public void SelectPage()
	{
		EventBus.RaiseEvent(delegate(IColonizationProjectsUIHandler x)
		{
			x.HandleColonyProjectPage(m_BlueprintColonyProject);
		});
	}

	public void SetSelection(BlueprintColonyProject blueprintColonyProject)
	{
		IsSelected.Value = m_BlueprintColonyProject == blueprintColonyProject;
	}

	protected override void DisposeImplementation()
	{
	}

	private void CalculateProgress()
	{
		if (m_ColonyProject != null && m_BlueprintColonyProject.SegmentsToBuild != 0)
		{
			SegmentsToBuild.Value = m_BlueprintColonyProject.SegmentsToBuild;
			Progress.Value = (Game.Instance.TimeController.GameTime - m_ColonyProject.StartTime).TotalSegments();
		}
	}

	private void UpdateBuildingState()
	{
		IsBuilding.Value = m_ColonyProject != null && !m_ColonyProject.IsFinished;
	}

	private void UpdateFinishedState()
	{
		IsFinished.Value = m_ColonyProject != null && m_ColonyProject.IsFinished;
	}

	private void UpdateExcludedState()
	{
		IsExcluded.Value = m_Colony.ProjectIsExcluded(m_BlueprintColonyProject);
	}

	private void UpdateOtherProjectIsBuildingState()
	{
		OtherProjectIsBuilding.Value = !m_Colony.AllProjectsFinished();
	}

	private void UpdateMeetRequirementsState()
	{
		IsNotMeetRequirements.Value = m_ColonyProject == null && !m_Colony.ProjectMeetRequirements(m_BlueprintColonyProject);
	}

	private void UpdateColonyProject()
	{
		m_ColonyProject = m_Colony.Projects.FirstOrDefault((ColonyProject colonyProject) => colonyProject.Blueprint == m_BlueprintColonyProject);
	}
}
