using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitActionPointsHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleRestoreActionPoints();

	void HandleActionPointsSpent(BaseUnitEntity unit);
}
