using System;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

[Serializable]
public class TraumasModification : IHashable
{
	[JsonProperty]
	public int OldWoundDelayRoundsModifier;

	[JsonProperty]
	public int WoundStacksForTraumaModifier;

	[JsonProperty]
	public int WoundDamagePerTurnThresholdHPFractionModifier;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref OldWoundDelayRoundsModifier);
		result.Append(ref WoundStacksForTraumaModifier);
		result.Append(ref WoundDamagePerTurnThresholdHPFractionModifier);
		return result;
	}
}
