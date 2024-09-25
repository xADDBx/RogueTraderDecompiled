using System;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.SceneControllables;

[Serializable]
public class ControllableState : IHashable
{
	[JsonProperty]
	public bool? Active;

	[JsonProperty]
	public int? State;

	public ControllableState MergeWith(ControllableState otherState)
	{
		return new ControllableState
		{
			Active = (otherState.Active ?? Active),
			State = (otherState.State ?? State)
		};
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		if (Active.HasValue)
		{
			bool val = Active.Value;
			result.Append(ref val);
		}
		if (State.HasValue)
		{
			int val2 = State.Value;
			result.Append(ref val2);
		}
		return result;
	}
}
