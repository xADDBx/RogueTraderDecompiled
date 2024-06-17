namespace Kingmaker.PubSubSystem.Core.Interfaces;

public interface IHideUIWhileActionCameraHandler : ISubscriber
{
	void HandleHideUI();

	void HandleShowUI();
}
