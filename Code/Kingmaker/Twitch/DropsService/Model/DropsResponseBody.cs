using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Twitch.DropsService.Model;

public class DropsResponseBody
{
	[JsonProperty("reward_statuses")]
	public Dictionary<string, RewardStatus> RewardStatuses;
}
