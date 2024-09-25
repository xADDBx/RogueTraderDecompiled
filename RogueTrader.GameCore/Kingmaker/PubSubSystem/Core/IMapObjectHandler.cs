using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IMapObjectHandler : ISubscriber<IMapObjectEntity>, ISubscriber
{
	void HandleMapObjectSpawned();

	void HandleMapObjectDestroyed();
}
