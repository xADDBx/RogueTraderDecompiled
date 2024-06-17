using System;
using Kingmaker.UI.MVVM.VM.Colonization.Rewards;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.VM.Colonization;

public class ColonyRewardsOtherRewardVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<Sprite> Icon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<string> Description = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> CountText = new ReactiveProperty<string>();

	public readonly ReactiveProperty<TooltipBaseTemplate> Tooltip = new ReactiveProperty<TooltipBaseTemplate>();

	public ColonyRewardsOtherRewardVM(RewardUI rewardUI)
	{
		Icon.Value = rewardUI.Icon;
		Description.Value = rewardUI.Description;
		CountText.Value = rewardUI.CountText;
		Tooltip.Value = rewardUI.GetTooltip();
	}

	protected override void DisposeImplementation()
	{
	}
}
