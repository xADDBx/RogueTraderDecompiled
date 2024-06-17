using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IAreaTransitionHandler : ISubscriber
{
	void HandleAreaTransition();
}
