using Kingmaker.Interaction;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects;

namespace Kingmaker.PubSubSystem;

public interface IInteractWithVariantActorHandler : ISubscriber
{
	void HandleInteractWithVariantActor(InteractionPart interactionPart, IInteractionVariantActor variantActor);
}
