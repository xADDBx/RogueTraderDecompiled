using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[HashRoot]
public class RoamingPoint : IRoamingPoint, IHashable
{
	[JsonProperty]
	public float IdleTime;

	[CanBeNull]
	[JsonProperty]
	public Cutscene IdleCutscene;

	[JsonProperty(IsReference = false)]
	public Vector3 Position { get; internal set; }

	public float? Orientation => null;

	public TimeSpan SelectIdleTime(StatefulRandom random)
	{
		return IdleTime.Seconds();
	}

	public Cutscene SelectCutscene(StatefulRandom random)
	{
		return IdleCutscene;
	}

	public IRoamingPoint SelectNextPoint(StatefulRandom random)
	{
		return null;
	}

	public IRoamingPoint SelectPrevPoint(StatefulRandom random)
	{
		return null;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Vector3 val = Position;
		result.Append(ref val);
		result.Append(ref IdleTime);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(IdleCutscene);
		result.Append(ref val2);
		return result;
	}
}
