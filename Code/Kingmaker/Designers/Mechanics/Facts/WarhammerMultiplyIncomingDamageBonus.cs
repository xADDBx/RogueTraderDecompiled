using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("2d16a086cc16415a8016057b2c28a39e")]
public class WarhammerMultiplyIncomingDamageBonus : UnitBuffComponentDelegate, IHashable
{
	public float PercentIncreaseMultiplier = 1f;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
