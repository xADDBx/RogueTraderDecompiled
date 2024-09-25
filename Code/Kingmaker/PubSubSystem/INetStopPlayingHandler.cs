using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface INetStopPlayingHandler : ISubscriber
{
	void HandleStopPlaying();
}
