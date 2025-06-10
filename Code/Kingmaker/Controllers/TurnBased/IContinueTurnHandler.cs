using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Controllers.TurnBased;

public interface IContinueTurnHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitContinueTurn(bool isTurnBased);
}
