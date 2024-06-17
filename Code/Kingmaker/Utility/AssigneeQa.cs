using Newtonsoft.Json;

namespace Kingmaker.Utility;

public struct AssigneeQa
{
	[JsonProperty]
	public string Assignee;

	[JsonProperty(PropertyName = "QA")]
	public string Qa;
}
