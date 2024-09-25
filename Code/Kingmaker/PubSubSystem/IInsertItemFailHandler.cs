using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInsertItemFailHandler : ISubscriber<IItemEntity>, ISubscriber
{
	void HandleInsertFail(MechanicEntity owner);
}
