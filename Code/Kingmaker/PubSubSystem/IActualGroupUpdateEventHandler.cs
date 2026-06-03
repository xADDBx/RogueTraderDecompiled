using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IActualGroupUpdateEventHandler : ISubscriber
{
	void HandleActualGroupUpdate();
}
