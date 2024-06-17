using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Items;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardConsumableUI : RewardUI<RewardConsumable>
{
	public override string Name => string.Empty;

	public override string Description => base.Reward.Item.Name ?? "";

	public override Sprite Icon => base.Reward.Item?.Icon;

	public override string NameForAcronym => null;

	public override string CountText => "x" + base.Reward.MaxCount + " Max";

	public RewardConsumableUI(RewardConsumable reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			return (base.Reward.Item == null) ? null : new TooltipTemplateItem(base.Reward.Item.CreateEntity(), null, forceUpdateCache: false, replenishing: true);
		}
	}
}
