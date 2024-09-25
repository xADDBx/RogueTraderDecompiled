using System;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Learning.Collections;

[Serializable]
[HashRoot]
public readonly struct AttackData : IHashable
{
	[JsonProperty]
	public readonly string Ability;

	[JsonProperty]
	public readonly int Range;

	[JsonProperty]
	public readonly int Damage;

	public AttackData(string ability, int range, int damage)
	{
		Ability = ability;
		Range = range;
		Damage = damage;
	}

	public override string ToString()
	{
		return $"{GetType().Name}[{Ability}, Range: {Range}, Damage: {Damage}]";
	}

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(Ability);
		int val = Range;
		result.Append(ref val);
		int val2 = Damage;
		result.Append(ref val2);
		return result;
	}
}
