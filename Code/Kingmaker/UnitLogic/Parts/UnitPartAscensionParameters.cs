using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartAscensionParameters : BaseUnitPart, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, IHashable
{
	public Vector3 StartTurnPosition { get; set; }

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		StartTurnPosition = base.Owner.Position;
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		StartTurnPosition = base.Owner.Position;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
