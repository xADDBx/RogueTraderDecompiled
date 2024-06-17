using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Console.PS5.PSNObjects;

[JsonObject]
public class PSNObjectsManager : IHashable
{
	public void Activate()
	{
	}

	public void StartActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.IN_PROGRESS);
	}

	public void CompleteActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.COMPLETED);
	}

	public void FailActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.FAILED);
	}

	public void AbandonActivity(string activityId)
	{
		SetActivityStatus(activityId, ActivityStatus.ABANDONED);
	}

	private void SetActivityStatus(string activityId, ActivityStatus status)
	{
	}

	public virtual Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
