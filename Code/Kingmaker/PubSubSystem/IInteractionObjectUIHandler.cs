using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IInteractionObjectUIHandler : ISubscriber<IMapObjectEntity>, ISubscriber
{
	void HandleObjectHighlightChange();

	void HandleObjectInteractChanged();

	void HandleObjectInteract();
}
