using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICullFocusHandler : ISubscriber
{
	void HandleRemoveFocus();

	void HandleRestoreFocus();
}
