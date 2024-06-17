using System;
using Kingmaker.Blueprints.Items;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class Consumable : IHashable
{
	[JsonProperty]
	public BlueprintItem Item;

	[JsonProperty]
	public int SegmentsToRefill;

	[JsonProperty]
	public TimeSpan LastRefill;

	[JsonProperty]
	public int MaxCount;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Item);
		result.Append(ref val);
		result.Append(ref SegmentsToRefill);
		result.Append(ref LastRefill);
		result.Append(ref MaxCount);
		return result;
	}
}
