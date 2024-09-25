using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitSpawnHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitSpawned();
}
