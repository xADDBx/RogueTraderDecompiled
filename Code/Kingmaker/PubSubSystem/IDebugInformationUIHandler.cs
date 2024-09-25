using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IDebugInformationUIHandler : ISubscriber
{
	void HandleShowDebugBubble();

	void HandleHideDebugBubble();
}
