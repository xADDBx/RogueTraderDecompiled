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

	private readonly AutoDisposingList<IViewModel> m_RewardsAndRequirements = new AutoDisposingList<IViewModel>();

	protected override void DestroyViewImplementation()
	{
		m_RewardsAndRequirements.Clear();
		m_RewardsWidgetList.Clear();
		base.DestroyViewImplementation();
	}

	protected override void DrawRewardsImpl()
	{
		m_RewardsAndRequirements.Clear();
		m_RewardsWidgetList.Clear();
		m_RewardsAndRequirements.Add(base.ViewModel.RequirementsHeader);
		m_RewardsAndRequirements.AddRange(base.ViewModel.Requirements);
		m_RewardsAndRequirements.Add(base.ViewModel.RewardsHeader);
		m_RewardsAndRequirements.AddRange(base.ViewModel.Rewards);
		m_RewardsWidgetList.DrawMultiEntries(m_RewardsAndRequirements, new List<IWidgetView> { m_HeaderViewPrefab, m_RequirementsViewPrefab, m_RewardsViewPrefab });
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
