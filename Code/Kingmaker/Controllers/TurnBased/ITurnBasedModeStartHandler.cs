using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface ITurnBasedModeStartHandler : ISubscriber
{
	void HandleTurnBasedModeStarted();
}
