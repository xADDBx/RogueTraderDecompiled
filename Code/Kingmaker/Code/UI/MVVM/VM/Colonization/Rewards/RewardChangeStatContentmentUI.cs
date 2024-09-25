using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardChangeStatContentmentUI : RewardUI<RewardChangeStatContentment>
{
	public override string Name => string.Empty;

	public override string Description => UIStrings.Instance.ColonyProjectsRewards.RewardChangeStatContentment.Text;

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.Contentment;

	public override string NameForAcronym => null;

	public override int Count => base.Reward.ContentmentModifier;

	public override string CountText => base.Reward.ContentmentModifier.ToString("+#;-#;0");

	public override bool ApplyToAllColonies => base.Reward.ApplyToAllColonies;

	public override BlueprintColony Colony => base.Reward.Colony;

	public RewardChangeStatContentmentUI(RewardChangeStatContentment reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		UIColonizationTexts.ColonyStatsStrings statStrings = UIStrings.Instance.ColonizationTexts.GetStatStrings(2);
		return new TooltipTemplateSimple(ApplyToAllColonies ? (statStrings.Name.Text + " (" + UIStrings.Instance.ColonyProjectsRewards.ForAllColonies.Text + ")") : statStrings.Name.Text, statStrings.Description);
	}
}
