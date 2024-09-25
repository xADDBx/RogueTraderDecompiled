using Kingmaker.Code.UI.Models.Tooltip;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.UI.Models.Tooltip.Base;
using Owlcat.Runtime.UI.Tooltips;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization.Rewards;

public class RewardUI : IUIDataProvider, IUICountModifier
{
	protected Reward Reward;

	public virtual string Name => "Not implemented";

	public virtual string Description => "Not implemented";

	public virtual Sprite Icon => null;

	public virtual string NameForAcronym => "Not implemented";

	public virtual int Count => 0;

	public virtual string CountText => "Not implemented";

	public virtual Color? IconColor => null;

	public virtual bool ApplyToAllColonies => false;

	public virtual BlueprintColony Colony => null;

	public virtual TooltipBaseTemplate GetTooltip()
	{
		return null;
	}

	public RewardUI(Reward reward)
	{
		Reward = reward;
	}
}
public class RewardUI<T> : RewardUI where T : Reward
{
	protected new T Reward => (T)base.Reward;

	public RewardUI(T reward)
		: base(reward)
	{
	}
}
