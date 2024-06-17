using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICanAccessColonizationHandler : ISubscriber
{
	void HandleCanAccessColonization();
}
