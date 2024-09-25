using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums.Sound;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAnimationEventHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleAnimationEvent(MappedAnimationEventType eventType);
}
