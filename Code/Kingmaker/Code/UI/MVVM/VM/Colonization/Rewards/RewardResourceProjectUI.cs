using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardResourceProjectUI : RewardUI<RewardResourceProject>
{
	public override string Name => string.Empty;

	public override string Description => base.Reward.Resource.Name;

	public override Sprite Icon => base.Reward.Resource?.Icon;

	public override string NameForAcronym => base.Reward.Resource.Name;

	public override string CountText => "x" + base.Reward.Count;

	public RewardResourceProjectUI(RewardResourceProject reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(base.Reward.Resource.Name, base.Reward.Resource.Description);
	}
}
