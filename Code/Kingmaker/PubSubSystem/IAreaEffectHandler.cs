using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAreaEffectHandler : ISubscriber<IAreaEffectEntity>, ISubscriber
{
	void HandleAreaEffectSpawned();

	void HandleAreaEffectDestroyed();
}
