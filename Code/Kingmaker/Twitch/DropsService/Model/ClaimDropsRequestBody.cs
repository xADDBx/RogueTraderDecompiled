using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Twitch.DropsService.Model;

public class ClaimDropsRequestBody
{
	[JsonProperty("rewards")]
	public List<string> Rewards;
}
