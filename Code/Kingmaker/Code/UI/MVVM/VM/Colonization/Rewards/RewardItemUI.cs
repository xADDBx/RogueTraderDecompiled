using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.Items;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Colonization.Rewards;

public class RewardItemUI : RewardUI<RewardItem>
{
	public override string Name => base.Reward.Item.Name;

	public override string Description => base.Reward.Item.Name;

	public override Sprite Icon => base.Reward.Item?.Icon;

	public override string NameForAcronym => null;

	public override string CountText => "x" + base.Reward.Count;

	public override int Count => base.Reward.Count;

	public bool IsMiner => base.Reward.IsMiner;

	public RewardItemUI(RewardItem reward)
		: base(reward)
	{
	}

	public override TooltipBaseTemplate GetTooltip()
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			return (base.Reward.Item == null) ? null : new TooltipTemplateItem(base.Reward.Item.CreateEntity());
		}
	}
}
