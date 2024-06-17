using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardResourceNotFromColonyUI : RewardUI<RewardResourceNotFromColony>
{
	public override string Name => base.Reward.Resource.Name;

	public override string Description => base.Reward.Resource.Name;

	public override Sprite Icon => base.Reward.Resource?.Icon;

	public override string NameForAcronym => null;

	public override string CountText => "x" + base.Reward.Count;

	public RewardResourceNotFromColonyUI(RewardResourceNotFromColony reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(base.Reward.Resource.Name, base.Reward.Resource.Description);
	}
}
