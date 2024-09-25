using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSneakAttackModifications : BaseUnitPart, IHashable
{
	public int AdditionalDamagePerDice;

	public int AdditionalDamagePerRangedDice;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
