using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IPauseHandler : ISubscriber
{
	void OnPauseToggled();
}
