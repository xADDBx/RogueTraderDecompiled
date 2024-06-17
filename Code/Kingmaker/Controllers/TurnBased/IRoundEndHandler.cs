using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IRoundEndHandler : ISubscriber
{
	void HandleRoundEnd(bool isTurnBased);
}
