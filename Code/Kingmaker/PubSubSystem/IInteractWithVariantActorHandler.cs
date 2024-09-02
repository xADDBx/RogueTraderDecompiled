using Kingmaker.Interaction;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.InteractionComponentBase;

namespace Kingmaker.PubSubSystem;

public interface IInteractWithVariantActorHandler : ISubscriber
{
	void HandleInteractWithVariantActor(InteractionPart interactionPart, IInteractionVariantActor variantActor);
}
