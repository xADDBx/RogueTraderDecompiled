using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker;

public class InGameSettings
{
	[JsonProperty]
	public Dictionary<string, object> List = new Dictionary<string, object>();
}
