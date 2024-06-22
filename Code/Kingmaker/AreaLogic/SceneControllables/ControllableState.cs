using System;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
public class ControllableState : IHashable
{
	[JsonProperty]
	public bool Active;

	[JsonProperty]
	public int State;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Active);
		result.Append(ref State);
		return result;
	}
}
