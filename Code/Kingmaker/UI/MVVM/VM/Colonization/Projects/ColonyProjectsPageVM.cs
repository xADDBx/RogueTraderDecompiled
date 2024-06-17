using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.Colonization.Requirements;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Projects;

public class ColonyProjectsPageVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IColonizationProjectsUIHandler, ISubscriber
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> Title = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> Description = new ReactiveProperty<string>();

	public readonly ReactiveCommand RefreshData = new ReactiveCommand();

	public readonly AutoDisposingList<ColonyProjectsRewardElementVM> Rewards = new AutoDisposingList<ColonyProjectsRewardElementVM>();

	public readonly AutoDisposingList<ColonyProjectsRequirementElementVM> Requirements = new AutoDisposingList<ColonyProjectsRequirementElementVM>();

	public readonly ColonyProjectsHeaderElementVM RewardsHeader = new ColonyProjectsHeaderElementVM(UIStrings.Instance.ColonyProjectsTexts.ProjectRewards.Text);

	public readonly ColonyProjectsHeaderElementVM RequirementsHeader = new ColonyProjectsHeaderElementVM(UIStrings.Instance.ColonyProjectsTexts.ProjectRequirements.Text);

	private BlueprintColonyProject m_BlueprintColonyProject;

	private Colony m_Colony;

	private IDisposable m_EventBusDispose;

	public BlueprintColonyProject BlueprintColonyProject => m_BlueprintColonyProject;

	protected override void DisposeImplementation()
	{
		Rewards.Clear();
		Requirements.Clear();
		m_EventBusDispose?.Dispose();
		m_EventBusDispose = null;
		m_Colony = null;
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

	public void HandleColonyProjectsUIOpen(Colony colony)
	{
		List<BlueprintColonyProject> list = colony.Blueprint.Projects.Dereference().ToList();
		Refresh(list[0]);
	}

	public void HandleColonyProjectsUIClose()
	{
	}

	public void HandleColonyProjectPage(BlueprintColonyProject blueprintColonyProject)
	{
		if (blueprintColonyProject != m_BlueprintColonyProject)
		{
			Refresh(blueprintColonyProject);
		}
	}

	private void Refresh(BlueprintColonyProject blueprintColonyProject)
	{
		if (m_Colony == null)
		{
			PFLog.System.Error("ColonyProjectsPageVM.Refresh - colony is null!");
			return;
		}
		Rewards.Clear();
		Requirements.Clear();
		m_BlueprintColonyProject = blueprintColonyProject;
		Icon.Value = blueprintColonyProject.Icon;
		Title.Value = blueprintColonyProject.Name;
		Description.Value = blueprintColonyProject.Description;
		foreach (Reward component in blueprintColonyProject.GetComponents<Reward>())
		{
			ColonyProjectsRewardElementVM item = new ColonyProjectsRewardElementVM(component);
			Rewards.Add(item);
		}
		foreach (Requirement component2 in blueprintColonyProject.GetComponents<Requirement>())
		{
			ColonyProjectsRequirementElementVM item2 = new ColonyProjectsRequirementElementVM(component2, m_Colony);
			Requirements.Add(item2);
		}
		RefreshData.Execute();
	}
}
