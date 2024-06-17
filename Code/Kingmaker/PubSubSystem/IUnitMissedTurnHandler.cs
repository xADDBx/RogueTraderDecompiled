using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitMissedTurnHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleOnMissedTurn();
}
