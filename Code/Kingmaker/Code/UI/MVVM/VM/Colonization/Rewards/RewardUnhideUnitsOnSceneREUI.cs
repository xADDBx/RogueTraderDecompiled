using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardUnhideUnitsOnSceneREUI : RewardUI<RewardUnhideUnitsOnSceneRE>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardUnhideUnitsOnSceneRE.Text;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RewardUnhideUnitsOnSceneREUI(RewardUnhideUnitsOnSceneRE reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.ColonyProjectsRewards.RewardUnhideUnitsOnSceneRE.Text, UIStrings.Instance.ColonyProjectsRewards.RewardUnhideUnitsOnSceneREDesc.Text);
	}
}
