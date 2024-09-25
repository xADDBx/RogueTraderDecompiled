using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Progression.Paths;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UI;

[JsonObject]
public struct SavedUnitProgressionWindowData : IHashable
{
	[JsonProperty]
	public BlueprintCareerPath.Reference CareerPath;

	public Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.BlueprintReferenceHasher.GetHash128(CareerPath);
		result.Append(ref val);
		return result;
	}
}
