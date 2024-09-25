using Newtonsoft.Json;

namespace Kingmaker.Modding;

public class WriteableSettingsData
{
	[JsonProperty]
	public string[] SourceDirectories = new string[0];

	[JsonProperty]
	public string[] EnabledModifications = new string[0];
}
