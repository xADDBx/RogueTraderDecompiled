using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEntityRevealedHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleEntityRevealed();
}
