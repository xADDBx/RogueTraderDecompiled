using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAgumentationsDollRotationHandler : ISubscriber
{
	void HandleOnRotationStop();
}
