using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization.Rewards;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class ColonyChronicle : IHashable
{
	[JsonProperty]
	public TimeSpan Date;

	[JsonProperty]
	public BlueprintColonyChronicle Blueprint { get; private set; }

	public ColonyChronicle(BlueprintColonyChronicle blueprint, TimeSpan date)
	{
		Blueprint = blueprint;
		Date = date;
	}

	public ColonyChronicle(JsonConstructorMark _)
	{
	}

	public void ReceiveReward(Colony colony)
	{
		foreach (Reward item in Blueprint.GetComponents<Reward>().EmptyIfNull())
		{
			item.ReceiveReward(colony);
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		result.Append(ref Date);
		return result;
	}
}
