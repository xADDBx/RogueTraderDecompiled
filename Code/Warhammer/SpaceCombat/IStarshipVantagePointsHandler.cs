using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Warhammer.SpaceCombat;

public interface IStarshipVantagePointsHandler : ISubscriber<IStarshipEntity>, ISubscriber
{
	void HandleEnteredVantagePoint();

	void HandleLeavedVantagePoint();
}
public interface IStarshipVantagePointsHandler<TTag> : IStarshipVantagePointsHandler, ISubscriber<IStarshipEntity>, ISubscriber, IEventTag<IStarshipVantagePointsHandler, TTag>
{
}
