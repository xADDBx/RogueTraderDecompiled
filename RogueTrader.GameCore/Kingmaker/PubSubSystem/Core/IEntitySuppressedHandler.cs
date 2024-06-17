using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IEntitySuppressedHandler : ISubscriber<IEntity>, ISubscriber
{
	void HandleEntitySuppressionChanged(IEntity entity, bool suppressed);
}
