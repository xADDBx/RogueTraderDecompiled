using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Sound;

public interface IAudioZoneHandler : ISubscriber
{
	void HandleListenerZone(AudioZone zone, bool isInside);
}
