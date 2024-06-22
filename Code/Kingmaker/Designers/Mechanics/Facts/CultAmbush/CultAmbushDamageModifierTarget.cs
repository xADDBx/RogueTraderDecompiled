using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Damage;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.CultAmbush;

[AllowMultipleComponents]
[TypeId("b952465c104f41e6802093ba4366ec40")]
public class CultAmbushDamageModifierTarget : WarhammerDamageModifier, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

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
