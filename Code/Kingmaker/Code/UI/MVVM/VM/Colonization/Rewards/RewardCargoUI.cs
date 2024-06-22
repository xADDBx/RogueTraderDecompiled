using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardCargoUI : RewardUI<RewardCargo>
{
	public override string Name => base.Reward.Cargo.Name;

	public override string Description
	{
		get
		{
			if (base.Reward.Cargo != null)
			{
				return base.Reward.Cargo.Name;
			}
			return string.Empty;
		}
	}

	public override Sprite Icon => BlueprintRoot.Instance.UIConfig.UIIcons.CargoTooltipIcons.GetIconByOrigin(base.Reward.Cargo.OriginType);

	public override string NameForAcronym => null;

	public override string CountText => "x" + base.Reward.Count;

	public override Color? IconColor => Color.white;

	public RewardCargoUI(RewardCargo reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		if (base.Reward.Cargo == null)
		{
			PFLog.UI.Error("RewardCargo.GetUITooltip - Cargo is null!");
			return null;
		}
		return new TooltipTemplateCargo(base.Reward.Cargo);
	}
}
