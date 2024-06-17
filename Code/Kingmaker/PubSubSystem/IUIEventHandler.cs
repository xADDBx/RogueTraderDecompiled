using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUIEventHandler : ISubscriber
{
	void HandleUIEvent(UIEventType type);
}
