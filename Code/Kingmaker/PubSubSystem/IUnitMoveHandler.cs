using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitMoveHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitMovement(AbstractUnitEntity unit);
}
