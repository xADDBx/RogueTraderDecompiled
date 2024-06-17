using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFullScreenLocalMapUIHandler : ISubscriber
{
	void HandleFullScreenLocalMapChanged(bool state);
}
