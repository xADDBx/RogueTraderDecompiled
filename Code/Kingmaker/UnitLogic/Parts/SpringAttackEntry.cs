using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class SpringAttackEntry : IHashable
{
	[JsonProperty]
	public Vector3 OldPosition;

	[JsonProperty]
	public Vector3 NewPosition;

	[JsonProperty]
	public int Index;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref OldPosition);
		result.Append(ref NewPosition);
		result.Append(ref Index);
		return result;
	}
}
