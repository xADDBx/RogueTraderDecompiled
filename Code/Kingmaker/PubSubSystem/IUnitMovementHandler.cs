using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitMovementHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleWaypointUpdate(int index);

	void HandleMovementComplete();
}
