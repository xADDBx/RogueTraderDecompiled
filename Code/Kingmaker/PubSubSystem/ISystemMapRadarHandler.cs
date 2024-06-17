using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ISystemMapRadarHandler : ISubscriber
{
	void HandleShowSystemMapRadar();
}
