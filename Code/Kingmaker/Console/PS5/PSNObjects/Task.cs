using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.Console.PS5.PSNObjects;

[JsonObject]
public class Task : ActivityBase
{
	[JsonProperty]
	public List<SubTask> SubTasks = new List<SubTask>();

	public Task(string id, ActivityStatus status = ActivityStatus.NOT_STARTED)
		: base(id, status)
	{
	}

	[JsonConstructor]
	private Task()
	{
	}
}
