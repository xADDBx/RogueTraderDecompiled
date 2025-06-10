using System.Collections.Generic;

namespace Kingmaker.Interaction;

public interface IHasInteractionVariantActors
{
	bool InteractThroughVariants { get; }

	float OvertipCorrection { get; }

	IEnumerable<IInteractionVariantActor> GetInteractionVariantActors();
}
