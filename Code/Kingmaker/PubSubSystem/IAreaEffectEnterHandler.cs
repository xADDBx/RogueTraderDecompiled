using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAreaEffectEnterHandler : ISubscriber<IAreaEffectEntity>, ISubscriber
{
	void HandleUnitEnterAreaEffect(BaseUnitEntity unit);
}
