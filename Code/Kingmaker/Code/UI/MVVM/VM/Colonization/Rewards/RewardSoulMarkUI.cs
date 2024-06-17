using Kingmaker.Blueprints.Root;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Tooltip.Templates;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardSoulMarkUI : RewardUI<RewardSoulMark>
{
	public override string Name => string.Empty;

	public override string Description => UIUtility.GetSoulMarkDirectionText(base.Reward.SoulMarkShift.Direction).Text + " " + CountText;

	public override Sprite Icon => UIConfig.Instance.UIIcons.SoulMarkIcons.GetIconByDirection(base.Reward.SoulMarkShift.Direction);

	public override string NameForAcronym => null;

	public override string CountText => base.Reward.SoulMarkShift.Value.ToString("+#;-#;0");

	public RewardSoulMarkUI(RewardSoulMark reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		return new TooltipTemplateSoulMarkHeader(Game.Instance.Player.MainCharacterEntity, base.Reward.SoulMarkShift.Direction);
	}
}
