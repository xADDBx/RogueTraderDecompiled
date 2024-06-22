using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("f39fb0dcf8a742c28e1187843821e7f2")]
public class ContextActionCultAmbushMakeVisiblePassiveAbility : ContextAction
{
	public override string GetCaption()
	{
		return "CultAmbushMakeVisiblePassiveAbility";
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = base.Context?.MaybeOwner;
		BlueprintScriptableObject blueprintScriptableObject = base.Context?.AssociatedBlueprint;
		if (blueprintScriptableObject != null && mechanicEntity != null && blueprintScriptableObject is BlueprintFeature feature && mechanicEntity is BaseUnitEntity entity && entity.TryGetUnitPartCultAmbush(out var ambush))
		{
			ambush.Use(feature);
		}
	}
}
