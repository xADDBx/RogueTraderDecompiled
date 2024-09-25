using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitFootstepAnimationEventHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitFootstepAnimationEvent(string locator, int altFoot);
}
