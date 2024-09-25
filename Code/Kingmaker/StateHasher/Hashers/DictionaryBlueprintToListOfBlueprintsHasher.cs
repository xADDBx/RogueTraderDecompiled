using System.Collections.Generic;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.StateHasher.Hashers;

public static class DictionaryBlueprintToListOfBlueprintsHasher<T1, T2> where T1 : SimpleBlueprint where T2 : SimpleBlueprint
{
	public static Hash128 GetHash128(Dictionary<T1, List<T2>> obj)
	{
		Hash128 result = default(Hash128);
		if (obj == null)
		{
			return result;
		}
		int val = 0;
		foreach (KeyValuePair<T1, List<T2>> item in obj)
		{
			Hash128 hash = default(Hash128);
			Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Key);
			hash.Append(ref val2);
			if (item.Value != null)
			{
				for (int i = 0; i < item.Value.Count; i++)
				{
					Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(item.Value[i]);
					hash.Append(ref val3);
				}
			}
			val ^= hash.GetHashCode();
		}
		result.Append(ref val);
		return result;
	}
}
