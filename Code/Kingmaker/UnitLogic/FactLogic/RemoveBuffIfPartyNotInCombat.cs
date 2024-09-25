using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("072a56da3cd48a1499c6ccaf28d5eb54")]
public class RemoveBuffIfPartyNotInCombat : UnitBuffComponentDelegate, IPartyCombatHandler, ISubscriber, IHashable
{
	public void HandlePartyCombatStateChanged(bool inCombat)
	{
		if (!inCombat)
		{
			base.Buff.MarkExpired();
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
