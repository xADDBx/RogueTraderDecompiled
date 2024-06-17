using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Visual;

public class BoneTransformData : IHashable
{
	[JsonProperty(IsReference = false)]
	public readonly Vector3 Position;

	[JsonProperty(IsReference = false)]
	public readonly Quaternion Rotation;

	[JsonConstructor]
	private BoneTransformData()
	{
	}

	public BoneTransformData(Vector3 position, Quaternion rotation)
	{
		Position = position;
		Rotation = rotation;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Vector3 val = Position;
		result.Append(ref val);
		Quaternion val2 = Rotation;
		result.Append(ref val2);
		return result;
	}
}
