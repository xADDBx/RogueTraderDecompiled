using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ITutorialTriggerTimerHandler : ISubscriber
{
	void HandleTimerStart();
}
