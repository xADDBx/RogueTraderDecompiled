using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IOnNewUnitOnPostHandler : ISubscriber
{
	void HandleNewUnit();
}
