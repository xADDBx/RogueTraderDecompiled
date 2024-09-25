using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("a831eee056b245c0aeb142bae796d90f")]
public class WarhammerOneUnitTurnBuff : UnitBuffComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public void HandleUnitStartTurn(bool isTurnBased)
	{
		base.Buff.Remove();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
