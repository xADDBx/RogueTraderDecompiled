using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IColonyProjectsUpdatedHandler : ISubscriber
{
	void HandleColonyProjectsUpdated();
}
