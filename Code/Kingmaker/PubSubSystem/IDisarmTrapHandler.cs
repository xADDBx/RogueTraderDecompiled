using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.PubSubSystem;

public interface IDisarmTrapHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleDisarmTrapSuccess(TrapObjectView trap);

	void HandleDisarmTrapFail(TrapObjectView trap);

	void HandleDisarmTrapCriticalFail(TrapObjectView trap);
}
