using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ITorpedoSpawnHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleTorpedoSpawn(BaseUnitEntity caster);
}
