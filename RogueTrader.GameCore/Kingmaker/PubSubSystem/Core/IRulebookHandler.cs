using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IRulebookHandler<in T> : ISubscriber where T : IRulebookEvent
{
	void OnEventAboutToTrigger(T evt);

	void OnEventDidTrigger(T evt);
}
