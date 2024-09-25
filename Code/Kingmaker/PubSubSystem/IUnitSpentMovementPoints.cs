using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitSpentMovementPoints<TTag> : IUnitSpentMovementPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitSpentMovementPoints, TTag>
{
}
public interface IUnitSpentMovementPoints : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitSpentMovementPoints(float movementPointsSpent);
}
