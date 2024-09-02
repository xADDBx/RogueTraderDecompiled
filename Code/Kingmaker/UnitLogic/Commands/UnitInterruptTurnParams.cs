using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Commands.Base;

namespace Kingmaker.UnitLogic.Commands;

public sealed class UnitInterruptTurnParams : UnitCommandParams<UnitInterruptTurn>
{
	public EntityRef<MechanicEntity> EntityToGetTheTurn;

	public EntityRef<MechanicEntity> InterruptionSource;

	public InterruptionData InterruptionData;

	public UnitInterruptTurnParams(MechanicEntity unit, MechanicEntity source, InterruptionData interruptionData)
	{
		EntityToGetTheTurn = unit;
		InterruptionSource = source;
		InterruptionData = interruptionData;
	}
}
