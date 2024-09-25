using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IRoamingTurnEndHandler : ISubscriber
{
	void HandleEndRoamingTurn();
}
