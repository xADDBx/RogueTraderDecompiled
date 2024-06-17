using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class ProfitFactorModifier : ColonyModifier, IHashable
{
	[JsonProperty]
	public ProfitFactorModifierType ModifierType;

	public bool IsNegative => Value < 0f;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref ModifierType);
		return result;
	}
}
