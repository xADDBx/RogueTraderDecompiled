using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInteractionHighlightUIHandler : ISubscriber
{
	void HandleHighlightChange(bool isOn);
}
