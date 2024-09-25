using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IPreparationTurnBeginHandler : ISubscriber
{
	void HandleBeginPreparationTurn(bool canDeploy);
}
