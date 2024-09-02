using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartMainCharacter : BaseUnitPart, IHashable
{
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	public bool Temporary { get; set; }

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		bool val2 = Temporary;
		result.Append(ref val2);
		return result;
	}
}
