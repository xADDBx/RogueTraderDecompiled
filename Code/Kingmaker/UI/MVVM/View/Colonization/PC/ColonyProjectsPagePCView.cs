using Kingmaker.UI.MVVM.View.Colonization.Base;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Colonization.PC;

public class ColonyProjectsPagePCView : ColonyProjectsPageBaseView
{
	[SerializeField]
	private ColonyProjectsRewardElementPCView m_RewardsViewPrefab;

	[SerializeField]
	private ColonyProjectsRequirementElementPCView m_RequirementsViewPrefab;

	protected override void DrawRewardsImpl()
	{
		m_RewardsWidgetList.DrawEntries(base.ViewModel.Rewards, m_RewardsViewPrefab);
	}

	protected override void DrawRequirementsImpl()
	{
		m_RequirementsWidgetList.DrawEntries(base.ViewModel.Requirements, m_RequirementsViewPrefab);
	}
}
