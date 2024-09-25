using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGlobalMapSpaceSystemInformationWindowHandler : ISubscriber
{
	void HandleShowSpaceSystemInformationWindow();

	void HandleHideSpaceSystemInformationWindow();
}
