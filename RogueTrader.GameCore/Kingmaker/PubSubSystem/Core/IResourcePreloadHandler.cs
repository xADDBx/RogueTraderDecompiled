using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IResourcePreloadHandler : ISubscriber
{
	void OnPreloadResources();
}
