using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("f03f91203bc2b894cbe36b291bf72a50")]
public class EnableRollBackTrackerPart : UnitBuffComponentDelegate, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, IHashable
{
	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null)
		{
			base.Owner.GetOrCreate<UnitRollBackTrackerPart>().CacheValues();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner && base.Owner != null && isTurnBased)
		{
			base.Owner.GetOrCreate<UnitRollBackTrackerPart>().CacheValues();
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.Remove<UnitRollBackTrackerPart>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
