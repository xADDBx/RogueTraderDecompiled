using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class ColonyStatModifier : ColonyModifier, IHashable
{
	[JsonProperty]
	public ColonyStatModifierType ModifierType;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref ModifierType);
		return result;
	}
}
