using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardReputationUI : RewardUI<RewardReputation>
{
	public override string Name => UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Reward.Faction);

	public override string Description => UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Reward.Faction);

	public override Sprite Icon => UIConfig.Instance.UIIcons.GetFactionIcon(base.Reward.Faction);

	public override string NameForAcronym => null;

	public override int Count => base.Reward.Reputation;

	public override string CountText => $"{base.Reward.Reputation:+#;-#;0} {UIStrings.Instance.Tooltips.ReputationPointsAbbreviation.Text}";

	public RewardReputationUI(RewardReputation reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSimple(UIStrings.Instance.CharacterSheet.GetFactionLabel(base.Reward.Faction), UIStrings.Instance.CharacterSheet.GetFactionDescription(base.Reward.Faction));
	}
}
