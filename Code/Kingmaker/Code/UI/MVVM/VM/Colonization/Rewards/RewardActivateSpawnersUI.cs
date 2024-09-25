using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardActivateSpawnersUI : RewardUI<RewardActivateSpawners>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardActivateSpawners.Text;

	public override Sprite Icon => null;

	public override string NameForAcronym => null;

	public override string CountText => null;

	public RewardActivateSpawnersUI(RewardActivateSpawners reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		string text = "";
		string text2 = "";
		switch (base.Reward.Type)
		{
		case RewardActivateSpawnersType.Imperial:
			text = UIStrings.Instance.ColonyProjectsRewards.RewardActivateSpawners.Text;
			text2 = UIStrings.Instance.ColonyProjectsRewards.RewardActivateSpawnersDesc.Text;
			break;
		case RewardActivateSpawnersType.Pirate:
			text = UIStrings.Instance.ColonyProjectsRewards.RewardActivateSpawnersPirate.Text;
			text2 = UIStrings.Instance.ColonyProjectsRewards.RewardActivateSpawnersPirateDesc.Text;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return new TooltipTemplateSimple(text, text2);
	}
}
