using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitChangeAttackFactionsHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitChangeAttackFactions(MechanicEntity unit);
}
