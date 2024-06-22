using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.DodgeChance;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.CultAmbush;

[TypeId("768fb28096c14130bf448ce170991310")]
public class CultAmbushDodgeChanceModifierDefender : WarhammerDodgeChanceModifier, IInitiatorRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		TryApply(evt);
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
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
