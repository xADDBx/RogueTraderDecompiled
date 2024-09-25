using System.Collections.Generic;
using Kingmaker.UI.MVVM.View.Colonization.Base;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Utility;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.Console;

public class ColonyProjectsPageConsoleView : ColonyProjectsPageBaseView
{
	[SerializeField]
	private ColonyProjectsRewardElementConsoleView m_RewardsViewPrefab;

	[SerializeField]
	private ColonyProjectsRequirementElementConsoleView m_RequirementsViewPrefab;

	[SerializeField]
	private ColonyProjectsHeaderElementConsoleView m_HeaderViewPrefab;

	public readonly AutoDisposingList<IViewModel> RewardsAndRequirements = new AutoDisposingList<IViewModel>();

	protected override void DestroyViewImplementation()
	{
		RewardsAndRequirements.Clear();
		m_RewardsWidgetList.Clear();
		base.DestroyViewImplementation();
	}

	protected override void DrawRewardsImpl()
	{
		RewardsAndRequirements.Clear();
		m_RewardsWidgetList.Clear();
		RewardsAndRequirements.Add(base.ViewModel.RewardsHeader);
		RewardsAndRequirements.AddRange(base.ViewModel.Rewards);
		RewardsAndRequirements.Add(base.ViewModel.RequirementsHeader);
		RewardsAndRequirements.AddRange(base.ViewModel.Requirements);
		m_RewardsWidgetList.DrawMultiEntries(RewardsAndRequirements, new List<IWidgetView> { m_HeaderViewPrefab, m_RewardsViewPrefab, m_RequirementsViewPrefab });
	}

	protected override void ScrollListInternal(IConsoleEntity entity)
	{
		if (!(entity is ColonyProjectsRewardElementBaseView colonyProjectsRewardElementBaseView))
		{
			if (entity is ColonyProjectsRequirementElementBaseView colonyProjectsRequirementElementBaseView)
			{
				m_RewardsScrollRect.EnsureVisibleVertical(colonyProjectsRequirementElementBaseView.transform as RectTransform, 50f, smoothly: false, needPinch: false);
			}
		}
		else
		{
			m_RewardsScrollRect.EnsureVisibleVertical(colonyProjectsRewardElementBaseView.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
