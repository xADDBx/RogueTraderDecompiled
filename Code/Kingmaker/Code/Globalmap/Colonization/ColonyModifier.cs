using Kingmaker.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class ColonyModifier : IHashable
{
	[JsonProperty]
	public float Value;

	[JsonProperty]
	public BlueprintScriptableObject Modifier;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		result.Append(ref Value);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Modifier);
		result.Append(ref val);
		return result;
	}
}
