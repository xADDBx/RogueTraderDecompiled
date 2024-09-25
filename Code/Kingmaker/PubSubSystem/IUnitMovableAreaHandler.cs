using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Pathfinding;

namespace Kingmaker.PubSubSystem;

public interface IUnitMovableAreaHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleSetUnitMovableArea(List<GraphNode> nodes);

	void HandleRemoveUnitMovableArea();
}
