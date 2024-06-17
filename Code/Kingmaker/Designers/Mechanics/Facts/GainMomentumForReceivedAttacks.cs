using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67983016f052494993aa11a6d0406bfa")]
public class GainMomentumForReceivedAttacks : UnitFactComponentDelegate, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IHashable
{
	public int Bonus;

	public bool ScaleFromAttackerDifficulty;

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		if (evt.Initiator is UnitEntity unitEntity && !unitEntity.IsAlly(base.Owner))
		{
			AddMomentum(unitEntity);
		}
	}

	private void AddMomentum(UnitEntity initiator)
	{
		int num = (int)(initiator.Blueprint.DifficultyType + 1);
		int value = (ScaleFromAttackerDifficulty ? (Bonus * num) : Bonus);
		BaseUnitEntity owner = base.Owner;
		RuleReason reason = base.Context;
		Rulebook.Trigger(RulePerformMomentumChange.CreateCustom(owner, value, in reason));
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		BlueprintScriptableObject blueprintScriptableObject = evt.Reason.Context?.AssociatedBlueprint;
		AbilityData sourceAbility = evt.SourceAbility;
		if (evt.Initiator is UnitEntity initiator && !(blueprintScriptableObject is BlueprintBuff) && !(blueprintScriptableObject is BlueprintAbilityAreaEffect) && (sourceAbility?.Weapon == null || (object)sourceAbility == null || sourceAbility.Blueprint.AbilityParamsSource != WarhammerAbilityParamsSource.Weapon))
		{
			AddMomentum(initiator);
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
