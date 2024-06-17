using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface IInteractionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void OnInteract(InteractionPart interaction);

	void OnInteractionRestricted(InteractionPart interaction);
}
