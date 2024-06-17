using System.Collections.Generic;

namespace Kingmaker.Interaction;

public interface IHasInteractionVariantActors
{
	bool InteractThroughVariants { get; }

	IEnumerable<IInteractionVariantActor> GetInteractionVariantActors();
}
