using System.Collections.Generic;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.StateHasher.Hashers;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

public class CargoFromPointOfInterestHolder : IHashable
{
	[JsonProperty]
	public BasePointOfInterest Point;

	[JsonProperty]
	private List<BlueprintCargo> m_Cargo;

	[JsonProperty]
	public StarSystemObjectEntity Owner;

	public string Name => Owner.Name;

	public CargoFromPointOfInterestHolder()
	{
	}

	public CargoFromPointOfInterestHolder(BasePointOfInterest pointOfInterest, List<BlueprintCargo> cargoCollection, StarSystemObjectEntity entity)
	{
		Point = pointOfInterest;
		Owner = entity;
		m_Cargo = cargoCollection;
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = ClassHasher<BasePointOfInterest>.GetHash128(Point);
		result.Append(ref val);
		List<BlueprintCargo> cargo = m_Cargo;
		if (cargo != null)
		{
			for (int i = 0; i < cargo.Count; i++)
			{
				Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(cargo[i]);
				result.Append(ref val2);
			}
		}
		Hash128 val3 = ClassHasher<StarSystemObjectEntity>.GetHash128(Owner);
		result.Append(ref val3);
		return result;
	}
}
