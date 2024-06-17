using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IRoundStartHandler : ISubscriber
{
	void HandleRoundStart(bool isTurnBased);
}
