using Kingmaker.AreaLogic.SummonPool;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISummonPoolHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleUnitAdded(ISummonPool pool);

	void HandleUnitRemoved(ISummonPool pool);

	void HandleLastUnitRemoved(ISummonPool pool);
}
