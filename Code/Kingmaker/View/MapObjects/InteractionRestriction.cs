using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Interaction;

namespace Kingmaker.View.MapObjects;

public abstract class InteractionRestriction<TPart> : EntityPartComponent<TPart> where TPart : ViewBasedPart, new()
{
}
public abstract class InteractionRestriction<TPart, TSettings> : EntityPartComponent<TPart, TSettings> where TPart : ViewBasedPart, IInteractionVariantActor, new() where TSettings : class, new()
{
}
