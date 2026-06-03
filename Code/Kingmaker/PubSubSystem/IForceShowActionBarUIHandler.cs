using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IForceShowActionBarUIHandler : ISubscriber
{
	void HandleForceShowActionBar(bool state);
}
