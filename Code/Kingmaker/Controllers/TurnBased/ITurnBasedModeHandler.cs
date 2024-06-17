using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface ITurnBasedModeHandler : ISubscriber
{
	void HandleTurnBasedModeSwitched(bool isTurnBased);
}
