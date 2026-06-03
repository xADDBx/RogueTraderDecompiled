using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardCombativityUI : RewardUI<RewardCombativity>
{
	public override string Name => UIStrings.Instance.ProfitFactorTexts.CombativityTitle;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardCombativity.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Combativity;

	public override string NameForAcronym => null;

	public override int Count => base.Reward.Combativity;

	public override string CountText => base.Reward.Combativity.ToString("+#;-#;0");

	public RewardCombativityUI(RewardCombativity reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.ProfitFactorTexts.CombativityTitle.Text, UIStrings.Instance.ProfitFactorTexts.CombativityDescription.Text);
	}
}
