using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardStartContractUI : RewardUI<RewardStartContract>
{
	public override string Name => string.Empty;

	public override string Description => GetDescription();

	public override Sprite Icon => UIConfig.Instance.UIIcons.Reward;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RewardStartContractUI(RewardStartContract reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		if (base.Reward.Objective != null)
		{
			return new TooltipTemplateSimple(base.Reward.Objective.Title.Text, base.Reward.Objective.Description.Text);
		}
		return null;
	}

	private string GetDescription()
	{
		if (base.Reward.Objective == null)
		{
			PFLog.UI.Error("RewardStartContract.GetUIText - Objective is null!");
			return null;
		}
		return string.Format(UIStrings.Instance.ColonyProjectsRewards.RewardStartContract, base.Reward.Objective.Title.Text);
	}
}
