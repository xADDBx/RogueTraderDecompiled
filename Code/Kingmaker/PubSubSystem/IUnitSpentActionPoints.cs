using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitSpentActionPoints<TTag> : IUnitSpentActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitSpentActionPoints, TTag>
{
}
public interface IUnitSpentActionPoints : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitSpentActionPoints(int actionPointsSpent);
}
