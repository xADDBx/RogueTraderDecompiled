using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Interaction;

namespace Kingmaker.Designers.EventConditionActionSystem.ContextData;

public class InteractionVariantData : ContextData<InteractionVariantData>
{
	public IInteractionVariantActor VariantActor { get; private set; }

	public InteractionVariantData Setup(IInteractionVariantActor variantActor)
	{
		VariantActor = variantActor;
		return this;
	}

	protected override void Reset()
	{
		VariantActor = null;
	}
}
