using System;
using System.Collections.Generic;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Etudes;

[Serializable]
public class MapToPlanets : IHashable
{
	[JsonProperty]
	public BlueprintStarSystemMap Map;

	[JsonProperty]
	public List<BlueprintPlanet> Planets;

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(Map);
		result.Append(ref val);
		List<BlueprintPlanet> planets = Planets;
		if (planets != null)
		{
			for (int i = 0; i < planets.Count; i++)
			{
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(planets[i]);
				result.Append(ref val2);
			}
		}
		return result;
	}
}
