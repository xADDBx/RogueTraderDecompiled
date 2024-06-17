using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("9e5d6fea90c0cb9418a322c839a11cf8")]
public class WarhammerCooldown : MechanicEntityFactComponentDelegate, IHashable
{
	[HideIf("UntilEndOfCombat")]
	public int CooldownInRounds;

	public bool UntilEndOfCombat;

	public int GetCooldown()
	{
		if (!UntilEndOfCombat)
		{
			return CooldownInRounds;
		}
		return int.MaxValue;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
