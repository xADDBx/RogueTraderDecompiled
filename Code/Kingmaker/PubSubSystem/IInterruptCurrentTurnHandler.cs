using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInterruptCurrentTurnHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleOnInterruptCurrentTurn();
}
