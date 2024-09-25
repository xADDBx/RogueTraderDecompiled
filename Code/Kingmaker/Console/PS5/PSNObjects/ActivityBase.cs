using Newtonsoft.Json;

namespace Kingmaker.Console.PS5.PSNObjects;

public abstract class ActivityBase
{
	[JsonProperty]
	public readonly string Id;

	[JsonProperty]
	public ActivityStatus Status { get; set; }

	protected ActivityBase(string id, ActivityStatus status = ActivityStatus.NOT_STARTED)
	{
		Id = id;
		Status = status;
	}

	[JsonConstructor]
	protected ActivityBase()
	{
	}
}
