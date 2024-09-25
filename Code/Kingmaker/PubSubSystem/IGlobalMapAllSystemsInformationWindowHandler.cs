using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGlobalMapAllSystemsInformationWindowHandler : ISubscriber
{
	void HandleShowAllSystemsInformationWindow();

	void HandleHideAllSystemsInformationWindow();
}
