using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.PubSubSystem;

public interface IUnitAbilityNonPushForceMoveHandler : ISubscriber
{
	void HandleUnitNonPushForceMove(int distanceInCells, MechanicsContext context, UnitEntity movedTarget);
}
