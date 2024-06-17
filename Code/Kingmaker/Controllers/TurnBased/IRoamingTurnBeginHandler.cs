using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IRoamingTurnBeginHandler : ISubscriber
{
	void HandleBeginRoamingTurn();
}
