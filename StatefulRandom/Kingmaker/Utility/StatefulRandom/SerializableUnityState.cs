using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Utility.StatefulRandom;

[JsonObject(IsReference = false)]
public struct SerializableUnityState
{
	[JsonProperty]
	public string StateJson;

	public UnityEngine.Random.State Value
	{
		get
		{
			if (string.IsNullOrEmpty(StateJson))
			{
				return GetDefaultState();
			}
			try
			{
				return JsonUtility.FromJson<UnityEngine.Random.State>(StateJson);
			}
			catch (Exception arg)
			{
				Debug.LogError($"UnityRandom.State can't be parsed! json='{StateJson}'\n{arg}");
				return GetDefaultState();
			}
		}
		set
		{
			StateJson = JsonUtility.ToJson(value);
		}
	}

	public SerializableUnityState(UnityEngine.Random.State unityRandomState)
	{
		StateJson = null;
		Value = unityRandomState;
	}

	private static UnityEngine.Random.State GetDefaultState()
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(0);
		UnityEngine.Random.State state2 = UnityEngine.Random.state;
		UnityEngine.Random.state = state;
		return state2;
	}
}
