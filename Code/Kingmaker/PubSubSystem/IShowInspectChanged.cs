using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IShowInspectChanged : ISubscriber
{
	void HandleShowInspect(bool state);
}
