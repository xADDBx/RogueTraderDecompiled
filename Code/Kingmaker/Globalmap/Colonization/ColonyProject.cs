using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Colonization;

public class ColonyProject : IHashable
{
	[JsonProperty]
	public BlueprintColonyProject Blueprint;

	[JsonProperty]
	public TimeSpan StartTime;

	[JsonProperty]
	public bool IsFinished;

	[JsonProperty]
	public Dictionary<BlueprintResource, int> UsedResourcesFromPool = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public Dictionary<BlueprintResource, int> ProducedResourcesWithoutModifiers = new Dictionary<BlueprintResource, int>();

	[JsonProperty]
	public Dictionary<BlueprintResource, int> ResourceShortage = new Dictionary<BlueprintResource, int>();

	public ColonyProject(BlueprintColonyProject blueprint)
	{
		Blueprint = blueprint;
		IsFinished = false;
	}

	public ColonyProject(JsonConstructorMark _)
	{
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		result.Append(ref StartTime);
		result.Append(ref IsFinished);
		Dictionary<BlueprintResource, int> usedResourcesFromPool = UsedResourcesFromPool;
		if (usedResourcesFromPool != null)
		{
			int val2 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item in usedResourcesFromPool)
			{
				Hash128 hash = default(Hash128);
				Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
				hash.Append(ref val3);
				int obj = item.Value;
				Hash128 val4 = UnmanagedHasher<int>.GetHash128(ref obj);
				hash.Append(ref val4);
				val2 ^= hash.GetHashCode();
			}
			result.Append(ref val2);
		}
		Dictionary<BlueprintResource, int> producedResourcesWithoutModifiers = ProducedResourcesWithoutModifiers;
		if (producedResourcesWithoutModifiers != null)
		{
			int val5 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item2 in producedResourcesWithoutModifiers)
			{
				Hash128 hash2 = default(Hash128);
				Hash128 val6 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item2.Key);
				hash2.Append(ref val6);
				int obj2 = item2.Value;
				Hash128 val7 = UnmanagedHasher<int>.GetHash128(ref obj2);
				hash2.Append(ref val7);
				val5 ^= hash2.GetHashCode();
			}
			result.Append(ref val5);
		}
		Dictionary<BlueprintResource, int> resourceShortage = ResourceShortage;
		if (resourceShortage != null)
		{
			int val8 = 0;
			foreach (KeyValuePair<BlueprintResource, int> item3 in resourceShortage)
			{
				Hash128 hash3 = default(Hash128);
				Hash128 val9 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item3.Key);
				hash3.Append(ref val9);
				int obj3 = item3.Value;
				Hash128 val10 = UnmanagedHasher<int>.GetHash128(ref obj3);
				hash3.Append(ref val10);
				val8 ^= hash3.GetHashCode();
			}
			result.Append(ref val8);
		}
		return result;
	}
}
