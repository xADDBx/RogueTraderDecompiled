using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UnitLogic.FactLogic;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a5cbfd1546727ec418590630a6ea2400")]
public class WarhammerDamageTriggerInitiator : WarhammerDamageTrigger, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ActionList Actions;

	public ActionList ActionsOnAttacker;

	public ContextPropertyName ContextPropertyName;

	public WarhammerKillTrigger.PropertyParameter PropertyToSave;

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	void IRulebookHandler<RuleDealDamage>.OnEventDidTrigger(RuleDealDamage rule)
	{
		TryTrigger(rule);
	}

	protected override void OnTrigger(RuleDealDamage rule)
	{
		if (base.Fact.MaybeContext != null)
		{
			if (PropertyToSave != 0)
			{
				if (PropertyToSave == WarhammerKillTrigger.PropertyParameter.EnemyDifficulty)
				{
					base.Context[ContextPropertyName] = ((int?)(rule.Target as UnitEntity)?.Blueprint.DifficultyType).GetValueOrDefault();
				}
				if (PropertyToSave == WarhammerKillTrigger.PropertyParameter.Damage)
				{
					base.Context[ContextPropertyName] = rule.Result;
				}
				if (PropertyToSave == WarhammerKillTrigger.PropertyParameter.DamageOverflow)
				{
					int hPBeforeDamage = rule.HPBeforeDamage;
					base.Context[ContextPropertyName] = Math.Max(rule.Result - hPBeforeDamage, 0);
				}
				if (PropertyToSave == WarhammerKillTrigger.PropertyParameter.Penetration)
				{
					base.Context[ContextPropertyName] = Math.Max(rule.Damage.Penetration.Value, 0);
				}
			}
			ActionList actions = Actions;
			if (actions != null && actions.HasActions)
			{
				base.Fact.RunActionInContext(Actions, rule.ConcreteTarget.ToITargetWrapper());
			}
			actions = ActionsOnAttacker;
			if (actions != null && actions.HasActions)
			{
				base.Fact.RunActionInContext(ActionsOnAttacker, rule.ConcreteInitiator.ToITargetWrapper());
			}
		}
		else
		{
			Actions?.Run();
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
