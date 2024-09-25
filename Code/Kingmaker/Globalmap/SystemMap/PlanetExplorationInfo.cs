using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class PlanetExplorationInfo : IHashable
{
	[JsonProperty]
	public BlueprintStarSystemMap StarSystemMap;

	[JsonProperty]
	public BlueprintPlanet Planet;

	[JsonProperty]
	public bool IsReportedToAdministratum;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(StarSystemMap);
		result.Append(ref val);
		Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Planet);
		result.Append(ref val2);
		result.Append(ref IsReportedToAdministratum);
		return result;
	}
}
