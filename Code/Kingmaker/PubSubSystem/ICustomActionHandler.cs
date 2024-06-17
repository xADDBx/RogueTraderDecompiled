using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICustomActionHandler : ISubscriber
{
	void HandleCustomEventRun(string id);
}
