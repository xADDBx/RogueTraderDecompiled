using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInitiativeTrackerShowGroup : ISubscriber
{
	void HandleShowChange();
}
