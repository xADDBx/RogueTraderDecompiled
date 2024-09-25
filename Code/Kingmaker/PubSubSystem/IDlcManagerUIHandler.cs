using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDlcManagerUIHandler : ISubscriber
{
	void HandleOpenDlcManager(bool inGame = false);

	void HandleCloseDlcManager();
}
