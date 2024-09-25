using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface ITimedEvent : ISubscriber
{
	void HandleTimePassed();
}
