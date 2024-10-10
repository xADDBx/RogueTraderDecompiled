using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.Twitch;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.UI.MVVM.VM.TwitchDrops;

public class TwitchDropsRewardsVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> IsAwaiting = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasItems = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HasStatus = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> StatusText = new ReactiveProperty<string>();

	public readonly ReactiveCommand UpdateRewards = new ReactiveCommand();

	public SlotsGroupVM<ItemSlotVM> SlotsGroup;

	private ItemsCollection m_Items;

	private Action m_CloseCallback;

	public TwitchDropsRewardsVM(Action closeCallback)
	{
		m_CloseCallback = closeCallback;
		m_Items = new ItemsCollection(null);
		GetRewards();
	}

	protected override void DisposeImplementation()
	{
	}

	public void Close()
	{
		m_CloseCallback?.Invoke();
		m_CloseCallback = null;
	}

	private async void GetRewards()
	{
		IsAwaiting.Value = true;
		TwitchDropsManager.DropsRewardsResult dropsRewardsResult = await TwitchDropsManager.Instance.GetRewardsAsync();
		if (dropsRewardsResult.RewardStatus == TwitchDropsClaimStatus.RewardsReceived)
		{
			ReceiveRewards(dropsRewardsResult.RewardItems);
		}
		else
		{
			SetStatus(dropsRewardsResult.RewardStatus);
		}
		IsAwaiting.Value = false;
	}

	private void ReceiveRewards(List<TwitchDropsManager.DropsRewardsResult.RewardItem> rewards)
	{
		m_Items.RemoveAll();
		foreach (TwitchDropsManager.DropsRewardsResult.RewardItem reward in rewards)
		{
			m_Items.Add(reward.Item, reward.Count);
		}
		HasItems.Value = m_Items != null && !m_Items.Empty();
		AddDisposable(SlotsGroup = new ItemSlotsGroupVM(m_Items, m_Items, 0, 0));
		UpdateRewards.Execute();
	}

	private void SetStatus(TwitchDropsClaimStatus status)
	{
		ReasonStrings reasons = LocalizedTexts.Instance.Reasons;
		string value = "";
		switch (status)
		{
		case TwitchDropsClaimStatus.Unknown:
		case TwitchDropsClaimStatus.PlatformNotSupported:
			value = reasons.UnavailableGeneric;
			break;
		case TwitchDropsClaimStatus.ConnectionError:
		case TwitchDropsClaimStatus.PlayerNotLinked:
			value = reasons.NoInternetConnection;
			break;
		case TwitchDropsClaimStatus.NoRewards:
			value = reasons.NoRewardsAvailable;
			break;
		case TwitchDropsClaimStatus.ReceivedAll:
			value = reasons.ReceivedAllRewards;
			break;
		default:
			throw new ArgumentOutOfRangeException("status", status, null);
		case TwitchDropsClaimStatus.RewardsReceived:
			break;
		}
		StatusText.Value = value;
		HasStatus.Value = true;
	}
}
