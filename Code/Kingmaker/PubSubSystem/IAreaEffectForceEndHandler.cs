using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAreaEffectForceEndHandler : ISubscriber<IAreaEffectEntity>, ISubscriber
{
	void HandleAreaEffectForceEndRequested();
}
