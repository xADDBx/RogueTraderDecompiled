using System.Collections.Generic;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class ExpeditionInfo : IHashable
{
	[JsonProperty]
	public BlueprintStarSystemObject StarSystemObject;

	[JsonProperty]
	public BlueprintPointOfInterestExpedition PointOfInterest;

	[JsonProperty]
	public List<LootEntry> Loot;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(StarSystemObject);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(PointOfInterest);
		result.Append(ref val2);
		List<LootEntry> loot = Loot;
		if (loot != null)
		{
			for (int i = 0; i < loot.Count; i++)
			{
				Hash128 val3 = ClassHasher<LootEntry>.GetHash128(loot[i]);
				result.Append(ref val3);
			}
		}
		return result;
	}
}
