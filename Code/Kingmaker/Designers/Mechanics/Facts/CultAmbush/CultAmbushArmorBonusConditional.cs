using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.CultAmbush;

[AllowMultipleComponents]
[TypeId("39674826e3bb4aeaad6a4a0891112f52")]
public class CultAmbushArmorBonusConditional : WarhammerArmorBonusConditional, IHashable
{
	protected override void OnApply()
	{
		base.OnApply();
		MechanicEntity mechanicEntity = base.Context?.MaybeOwner;
		BlueprintScriptableObject blueprintScriptableObject = base.Context?.AssociatedBlueprint;
		if (blueprintScriptableObject != null && mechanicEntity != null && blueprintScriptableObject is BlueprintFeature feature && mechanicEntity is BaseUnitEntity entity && entity.TryGetUnitPartCultAmbush(out var ambush))
		{
			ambush.Use(feature);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
