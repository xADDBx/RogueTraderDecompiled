using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IAudioSceneHandler : ISubscriber
{
	void OnAudioReloaded();
}
