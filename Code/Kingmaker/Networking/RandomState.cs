using System;
using Kingmaker.Utility.Random;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Networking;

[Serializable]
public class RandomState : IHashable
{
	[JsonProperty]
	public readonly int[] v;

	public static RandomState Instance { get; } = new RandomState();


	public RandomState()
	{
		v = new int[PFStatefulRandom.Serializable.Length];
	}

	public void Refresh()
	{
		int i = 0;
		for (int num = v.Length; i < num; i++)
		{
			v[i] = PFStatefulRandom.Serializable[i].State.GetHashCode();
		}
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(v);
		return result;
	}
}
