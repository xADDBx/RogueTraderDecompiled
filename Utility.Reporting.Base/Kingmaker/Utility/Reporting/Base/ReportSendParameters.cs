using Newtonsoft.Json;

namespace Kingmaker.Utility.Reporting.Base;

[JsonObject]
public class ReportSendParameters
{
	[JsonProperty]
	public string Priority;

	[JsonProperty]
	public string Version;

	[JsonProperty]
	public string Project;

	[JsonProperty]
	public string dev;

	[JsonProperty]
	public string Guid;

	[JsonProperty]
	public string Email;

	public override string ToString()
	{
		return JsonConvert.SerializeObject(this);
	}
}
