using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;

namespace Kingmaker.PubSubSystem;

public interface IUnitGainMovementPoints : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitGainMovementPoints(float movementPoints, MechanicsContext context);
}
public interface IUnitGainMovementPoints<TTag> : IUnitGainMovementPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitGainMovementPoints, TTag>
{
}
