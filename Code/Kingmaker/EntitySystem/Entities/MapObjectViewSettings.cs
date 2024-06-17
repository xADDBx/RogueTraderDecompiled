using Kingmaker.RandomEncounters.Settings;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class MapObjectViewSettings : IHashable
{
	[JsonProperty]
	public readonly BlueprintSpawnableObject Blueprint;

	[JsonProperty(IsReference = false)]
	public readonly Vector3 Position;

	[JsonProperty(IsReference = false)]
	public readonly Quaternion Rotation;

	[JsonConstructor]
	public MapObjectViewSettings(BlueprintSpawnableObject blueprint, Vector3 position, Quaternion rotation)
	{
		Blueprint = blueprint;
		Position = position;
		Rotation = rotation;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Blueprint);
		result.Append(ref val);
		Vector3 val2 = Position;
		result.Append(ref val2);
		Quaternion val3 = Rotation;
		result.Append(ref val3);
		return result;
	}
}
