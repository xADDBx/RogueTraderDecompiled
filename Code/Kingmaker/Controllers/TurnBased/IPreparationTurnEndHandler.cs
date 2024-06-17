using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IPreparationTurnEndHandler : ISubscriber
{
	void HandleEndPreparationTurn();
}
