using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Console.PS5.PSNObjects;

[JsonObject]
public class Activity : ActivityBase
{
	[JsonProperty]
	public List<Task> Tasks = new List<Task>();

	public Activity(string id, ActivityStatus status = ActivityStatus.NOT_STARTED)
		: base(id, status)
	{
	}

	[JsonConstructor]
	private Activity()
	{
	}
}
