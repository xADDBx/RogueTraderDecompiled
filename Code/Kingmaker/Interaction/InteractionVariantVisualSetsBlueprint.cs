using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Kingmaker.Interaction;

[TypeId("ebb14885bc11c134689104f2215091c2")]
public class InteractionVariantVisualSetsBlueprint : BlueprintScriptableObject
{
	[NotNull]
	[SerializeField]
	private List<InteractionVariantVisualSetEntry> Entries = new List<InteractionVariantVisualSetEntry>();

	public InteractionVariantVisualSetEntry GetSet(InteractionActorType type)
	{
		return Entries.FirstOrDefault((InteractionVariantVisualSetEntry e) => type == e.Type);
	}
}
