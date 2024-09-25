using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.PubSubSystem;

public interface ITrapActivationHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleTrapActivation(TrapObjectView trap);
}
