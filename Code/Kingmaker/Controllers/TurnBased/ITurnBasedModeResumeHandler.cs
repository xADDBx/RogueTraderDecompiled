using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface ITurnBasedModeResumeHandler : ISubscriber
{
	void HandleTurnBasedModeResumed();
}
