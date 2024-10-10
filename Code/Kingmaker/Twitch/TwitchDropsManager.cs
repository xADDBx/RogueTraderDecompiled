using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root;
using Kingmaker.EOSSDK;
using Kingmaker.Stores;
using Kingmaker.Twitch.DropsService;
using Kingmaker.Twitch.DropsService.Model;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Plugins.GOG;
using Steamworks;
using UnityEngine;

namespace Kingmaker.Twitch;

public class TwitchDropsManager
{
	public struct DropsRewardsResult
	{
		public struct RewardItem
		{
			public BlueprintItem Item;

			public int Count;
		}

		public TwitchDropsClaimStatus RewardStatus;

		public List<RewardItem> RewardItems;

		public DropsRewardsResult(TwitchDropsClaimStatus rewardStatus, List<RewardItem> rewardItems = null)
		{
			RewardStatus = rewardStatus;
			RewardItems = rewardItems;
		}
	}

	private static TwitchDropsManager s_Instance;

	private static readonly LogChannel Log = LogChannelFactory.GetOrCreate("TwitchDropsManager");

	private BlueprintTwitchRoot m_Config;

	private string m_GameUid;

	private List<string> m_AllAvailableDropIds;

	private DropsWebServiceClient m_WebServiceClient;

	private bool m_Initialized;

	private Task<DropsRewardsResult> m_DropsResultTask;

	private Task<TwitchDropsLinkStatus> m_DropsLinkStatusTask;

	private string m_FakeGameUid;

	public static TwitchDropsManager Instance => s_Instance ?? (s_Instance = new TwitchDropsManager());

	public bool IsPlatformSupported
	{
		get
		{
			TryInitialize();
			return !string.IsNullOrEmpty(m_GameUid);
		}
	}

	public string GameUid
	{
		get
		{
			return m_GameUid;
		}
		set
		{
			TryInitialize();
			m_GameUid = value;
			if (!string.IsNullOrEmpty(value))
			{
				if (m_WebServiceClient == null)
				{
					m_WebServiceClient = new DropsWebServiceClient(m_Config.DropsServiceUrl, m_Config.DropsServiceTimeoutSeconds);
				}
				if (m_AllAvailableDropIds == null)
				{
					m_AllAvailableDropIds = m_Config.DropsRewards.Select((BlueprintTwitchRoot.DropsReward dr) => dr.TwitchId).ToList();
				}
				m_DropsLinkStatusTask = null;
				m_DropsResultTask = null;
			}
			else
			{
				m_DropsLinkStatusTask = Task.FromResult(TwitchDropsLinkStatus.NotSupported);
				m_DropsResultTask = Task.FromResult(new DropsRewardsResult(TwitchDropsClaimStatus.PlatformNotSupported));
			}
		}
	}

	private TwitchDropsManager()
	{
	}

	public Task<TwitchDropsLinkStatus> GetTwitchLinkedStatus()
	{
		TryInitialize();
		if (m_DropsLinkStatusTask == null || (m_DropsLinkStatusTask.IsCompleted && m_DropsLinkStatusTask.Result != TwitchDropsLinkStatus.Linked && m_DropsLinkStatusTask.Result != TwitchDropsLinkStatus.NotSupported))
		{
			m_DropsLinkStatusTask = GetDropsStatusInternalAsync();
		}
		return m_DropsLinkStatusTask;
	}

	public Task<DropsRewardsResult> GetRewardsAsync()
	{
		TryInitialize();
		if (m_DropsResultTask == null || (m_DropsResultTask.IsCompleted && m_DropsResultTask.Result.RewardStatus != TwitchDropsClaimStatus.PlatformNotSupported))
		{
			m_DropsResultTask = GetDropsInternalAsync();
		}
		return m_DropsResultTask;
	}

	public void OpenTwitchLinkPage()
	{
		TryInitialize();
		if (m_WebServiceClient == null)
		{
			Log.Error("Platform not supported");
		}
		else
		{
			Application.OpenURL(m_WebServiceClient.GetLinkPageUri(m_GameUid).ToString());
		}
	}

	private void TryInitialize()
	{
		if (!m_Initialized)
		{
			if (!Game.HasInstance)
			{
				throw new Exception("Game not initialized");
			}
			m_Initialized = true;
			m_Config = BlueprintRoot.Instance.TwitchRoot;
			GameUid = GetGameUidFromPlatform();
		}
	}

	private async Task<TwitchDropsLinkStatus> GetDropsStatusInternalAsync()
	{
		DropsWebServiceClient.Response<DropsResponseBody> response = await m_WebServiceClient.GetDropsAsync(m_GameUid, null);
		if (response != null)
		{
			switch (response.StatusCode)
			{
			case HttpStatusCode.OK:
				return TwitchDropsLinkStatus.Linked;
			case HttpStatusCode.NotFound:
				return TwitchDropsLinkStatus.NotLinked;
			}
		}
		return TwitchDropsLinkStatus.ConnectionError;
	}

	private async Task<DropsRewardsResult> GetDropsInternalAsync()
	{
		DropsWebServiceClient.Response<DropsResponseBody> response = await m_WebServiceClient.GetDropsAsync(m_GameUid, m_AllAvailableDropIds);
		if (response != null)
		{
			switch (response.StatusCode)
			{
			case HttpStatusCode.OK:
			{
				DropsResponseBody dropsResponseBody = await response.GetBodyAsync();
				List<DropsRewardsResult.RewardItem> list = new List<DropsRewardsResult.RewardItem>();
				List<string> list2 = new List<string>();
				ProcessDropRewards(dropsResponseBody.RewardStatuses, list, list2);
				if (list2.Count > 0)
				{
					ConfirmRewardsClaimed(list2);
				}
				TwitchDropsClaimStatus rewardStatus = ((list.Count > 0) ? TwitchDropsClaimStatus.RewardsReceived : (PlayerHasUnclaimedRewards() ? TwitchDropsClaimStatus.NoRewards : TwitchDropsClaimStatus.ReceivedAll));
				return new DropsRewardsResult(rewardStatus, list);
			}
			case HttpStatusCode.NotFound:
				return new DropsRewardsResult(TwitchDropsClaimStatus.PlayerNotLinked);
			}
		}
		return new DropsRewardsResult(TwitchDropsClaimStatus.ConnectionError);
	}

	private void ProcessDropRewards(IReadOnlyDictionary<string, RewardStatus> rewardsStatusMap, ICollection<DropsRewardsResult.RewardItem> itemsAdded, ICollection<string> rewardsFulfilled)
	{
		Log.Log($"Processing {rewardsStatusMap.Count} reward ID's...");
		foreach (string rewardId in rewardsStatusMap.Keys)
		{
			RewardStatus rewardStatus = rewardsStatusMap[rewardId];
			if (rewardStatus == RewardStatus.Claimed || rewardStatus == RewardStatus.Fulfilled)
			{
				BlueprintTwitchRoot.DropsReward dropsReward = m_Config.DropsRewards.FirstOrDefault((BlueprintTwitchRoot.DropsReward dr) => dr.TwitchId == rewardId);
				if (dropsReward != null && TryGiveDropRewardToPlayer(dropsReward, itemsAdded) && rewardsStatusMap[rewardId] == RewardStatus.Claimed)
				{
					rewardsFulfilled.Add(rewardId);
				}
			}
		}
	}

	private async void ConfirmRewardsClaimed(IReadOnlyCollection<string> fulfilledRewards)
	{
		DropsWebServiceClient.Response<ClaimDropsResponseBody> response = await m_WebServiceClient.ClaimDropsAsync(m_GameUid, fulfilledRewards);
		if (response == null || !response.IsSuccessStatusCode)
		{
			Log.Error("Failed to claim rewards in Twitch API. See previous messages.");
		}
	}

	private static bool TryGiveDropRewardToPlayer(BlueprintTwitchRoot.DropsReward reward, ICollection<DropsRewardsResult.RewardItem> itemsAdded)
	{
		Player player = Game.Instance.Player;
		if (player.ClaimedTwitchDrops.Contains(reward.TwitchId))
		{
			return false;
		}
		foreach (BlueprintTwitchRoot.DropsRewardItem rewardItem in reward.RewardItems)
		{
			player.Inventory.Add(rewardItem.Item, rewardItem.Count);
			itemsAdded.Add(new DropsRewardsResult.RewardItem
			{
				Item = rewardItem.Item,
				Count = rewardItem.Count
			});
		}
		player.ClaimedTwitchDrops.Add(reward.TwitchId);
		return true;
	}

	private bool PlayerHasUnclaimedRewards()
	{
		Player player = Game.Instance.Player;
		return m_AllAvailableDropIds.Except(player.ClaimedTwitchDrops.EmptyIfNull()).Any();
	}

	private string GetGameUidFromPlatform()
	{
		switch (StoreManager.Store)
		{
		case StoreType.Steam:
			if (!SteamManager.Initialized)
			{
				Log.Error("Steam not initialized!");
				return null;
			}
			return $"steam_{SteamUser.GetSteamID().m_SteamID}";
		case StoreType.EpicGames:
			if (!EpicGamesManager.IsInitializedAndSignedIn())
			{
				Log.Error("EpicGamesManager not initialized or not signed in!");
				return null;
			}
			return $"egs_{EpicGamesManager.LocalUser.UserId}";
		case StoreType.GoG:
			if (!GogGalaxyManager.IsInitializedAndLoggedOn())
			{
				Log.Error("GogGalaxyManager not initialized or not signed in!");
				return null;
			}
			return $"gog_{GogGalaxyManager.Instance.UserId}";
		default:
			return null;
		}
	}
}
