using Kingmaker.Globalmap.Blueprints;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class AstropathBriefInfo : IHashable
{
	[JsonProperty]
	public BlueprintAstropathBrief AstropathBrief;

	[JsonProperty]
	public string MessageLocation;

	[JsonProperty]
	public string MessageDate;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(AstropathBrief);
		result.Append(ref val);
		result.Append(MessageLocation);
		result.Append(MessageDate);
		return result;
	}
}
