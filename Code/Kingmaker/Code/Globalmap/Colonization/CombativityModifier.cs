using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.Globalmap.Colonization;

public class CombativityModifier : ProfitFactorModifier, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
