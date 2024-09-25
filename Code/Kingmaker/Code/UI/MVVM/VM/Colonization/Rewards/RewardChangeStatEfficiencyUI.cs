using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardChangeStatEfficiencyUI : RewardUI<RewardChangeStatEfficiency>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardChangeStatEfficiency.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Efficiency;

	public override string NameForAcronym => null;

	public override int Count => base.Reward.EfficiencyModifier;

	public override string CountText => base.Reward.EfficiencyModifier.ToString("+#;-#;0");

	public override bool ApplyToAllColonies => base.Reward.ApplyToAllColonies;

	public override BlueprintColony Colony => base.Reward.Colony;

	public RewardChangeStatEfficiencyUI(RewardChangeStatEfficiency reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		UIColonizationTexts.ColonyStatsStrings statStrings = UIStrings.Instance.ColonizationTexts.GetStatStrings(0);
		return new TooltipTemplateSimple(ApplyToAllColonies ? (statStrings.Name.Text + " (" + UIStrings.Instance.ColonyProjectsRewards.ForAllColonies.Text + ")") : statStrings.Name.Text, statStrings.Description);
	}
}
