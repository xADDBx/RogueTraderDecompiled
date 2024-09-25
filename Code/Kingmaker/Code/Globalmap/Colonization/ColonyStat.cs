using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class ColonyStat : IHashable
{
	[JsonProperty]
	public int InitialValue;

	[JsonProperty]
	public int MinValue;

	[JsonProperty]
	public int MaxValue;

	[JsonProperty]
	public List<ColonyStatModifier> Modifiers = new List<ColonyStatModifier>();

	private int ModifiedValue => Modifiers.Aggregate(InitialValue, (int sum, ColonyStatModifier modifier) => sum += (int)modifier.Value);

	public int Value => Mathf.Clamp(ModifiedValue, MinValue, MaxValue);

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref InitialValue);
		result.Append(ref MinValue);
		result.Append(ref MaxValue);
		List<ColonyStatModifier> modifiers = Modifiers;
		if (modifiers != null)
		{
			for (int i = 0; i < modifiers.Count; i++)
			{
				Hash128 val = ClassHasher<ColonyStatModifier>.GetHash128(modifiers[i]);
				result.Append(ref val);
			}
		}
		return result;
	}
}
