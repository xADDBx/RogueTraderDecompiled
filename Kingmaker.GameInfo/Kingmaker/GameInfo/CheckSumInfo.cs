using Newtonsoft.Json;

namespace Kingmaker.GameInfo;

[JsonObject]
public class CheckSumInfo
{
	[JsonProperty]
	public string AssemblyName;

	[JsonProperty]
	public string Path;

	[JsonProperty]
	public string Sha1;
}
