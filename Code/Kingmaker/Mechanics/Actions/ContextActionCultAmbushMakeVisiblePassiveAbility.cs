using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("f39fb0dcf8a742c28e1187843821e7f2")]
public class ContextActionCultAmbushMakeVisiblePassiveAbility : ContextAction
{
	public enum Actors
	{
		Owner,
		Caster
	}

	[SerializeField]
	private Actors m_Actor;

	[SerializeField]
	private bool m_UseParentContext;

	public override string GetCaption()
	{
		return "CultAmbushMakeVisiblePassiveAbility";
	}

	protected override void RunAction()
	{
		MechanicEntity mechanicEntity = null;
		BlueprintScriptableObject blueprintScriptableObject = ((!m_UseParentContext) ? base.Context?.AssociatedBlueprint : base.Context?.ParentContext?.AssociatedBlueprint);
		switch (m_Actor)
		{
		case Actors.Owner:
			mechanicEntity = ((!m_UseParentContext) ? base.Context?.MaybeOwner : base.Context?.ParentContext?.MaybeOwner);
			break;
		case Actors.Caster:
			mechanicEntity = ((!m_UseParentContext) ? base.Context?.MaybeCaster : base.Context?.ParentContext?.MaybeCaster);
			break;
		}
		if (blueprintScriptableObject != null && mechanicEntity != null && blueprintScriptableObject is BlueprintFeature feature && mechanicEntity is BaseUnitEntity entity && entity.TryGetUnitPartCultAmbush(out var ambush))
		{
			ambush.Use(feature);
		}
	}
}
