using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("03664b38e7254bb1a8a6d1cde4953bcb")]
public class WarhammerDamageByDistanceModifier : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue DamageModifierPerCell = 0;

	public ContextValue PenetrationModifierPerCell = 0;

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		int distanceToTarget = evt.DistanceToTarget;
		evt.ValueModifiers.Add(ModifierType.ValAdd, DamageModifierPerCell.Calculate(base.Context) * distanceToTarget, evt, ModifierDescriptor.DistanceToTarget);
		evt.Penetration.Add(ModifierType.ValAdd, PenetrationModifierPerCell.Calculate(base.Context) * distanceToTarget, evt, ModifierDescriptor.DistanceToTarget);
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
