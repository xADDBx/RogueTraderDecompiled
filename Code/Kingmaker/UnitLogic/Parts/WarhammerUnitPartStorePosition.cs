using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class WarhammerUnitPartStorePosition : BaseUnitPart, IHashable
{
	[JsonProperty(IsReference = false)]
	public Vector3 storedPosition;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref storedPosition);
		return result;
	}
}
