using System.Linq;
using Core.Cheats;
using UnityEngine;

namespace Kingmaker.Twitch;

public class TwitchDropsTest
{
	[Cheat(Name = "twitch_check_linked", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static async void TestLinked()
	{
		Debug.Log($"Player linked status = {await TwitchDropsManager.Instance.GetTwitchLinkedStatus()}");
	}

	[Cheat(Name = "twitch_get_drops", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static async void TestGetDrops()
	{
		TwitchDropsManager.DropsRewardsResult dropsRewardsResult = await TwitchDropsManager.Instance.GetRewardsAsync();
		string arg = ((dropsRewardsResult.RewardStatus == TwitchDropsClaimStatus.RewardsReceived) ? string.Join(", ", dropsRewardsResult.RewardItems.Select((TwitchDropsManager.DropsRewardsResult.RewardItem it) => $"{it.Item.name} ({it.Count})")) : "N/A");
		Debug.Log($"Rewards status = {dropsRewardsResult.RewardStatus}, items = {arg}");
	}

	[Cheat(Name = "twitch_open_link_page", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void TestOpenLinkpage()
	{
		TwitchDropsManager.Instance.OpenTwitchLinkPage();
	}

	[Cheat(Name = "twitch_set_fake_uid", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SetFakeGameUid(string fakeUid = null)
	{
		Debug.Log("Setting Fake GameUid to " + fakeUid);
		TwitchDropsManager.Instance.GameUid = fakeUid;
	}
}
