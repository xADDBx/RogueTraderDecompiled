using Newtonsoft.Json;

namespace Kingmaker.Console.PS5.PSNObjects;

[JsonObject]
public class SubTask : ActivityBase
{
	public SubTask(string id, ActivityStatus status = ActivityStatus.NOT_STARTED)
		: base(id, status)
	{
	}

	[JsonConstructor]
	private SubTask()
	{
	}
}
