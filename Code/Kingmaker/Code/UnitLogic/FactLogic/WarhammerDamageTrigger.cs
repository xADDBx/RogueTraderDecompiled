using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[TypeId("e0ecefa49eeb4f80a63dba55e4f9dfd8")]
public abstract class WarhammerDamageTrigger : WarhammerDamageTriggerBase, IHashable
{
	public bool TriggerBeforeDamageHappens;

	public ActionList Actions;

	public ActionList ActionsOnAttacker;

	public ContextPropertyName ContextPropertyName;

	public WarhammerKillTrigger.PropertyParameter PropertyToSave;

	protected override void OnTrigger<TEvent>(TEvent rule)
	{
		if (base.Fact.MaybeContext == null)
		{
			Actions?.Run();
			return;
		}
		int? contextPropertyToSave = GetContextPropertyToSave(rule);
		if (contextPropertyToSave.HasValue)
		{
			base.Context[ContextPropertyName] = contextPropertyToSave.Value;
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

	private int? GetContextPropertyToSave<TEvent>(TEvent rule) where TEvent : RulebookTargetEvent, IDamageHolderRule
	{
		return PropertyToSave switch
		{
			WarhammerKillTrigger.PropertyParameter.EnemyDifficulty => ((int?)(rule.Target as UnitEntity)?.Blueprint.DifficultyType).GetValueOrDefault(), 
			WarhammerKillTrigger.PropertyParameter.Damage => GetDamage(rule), 
			WarhammerKillTrigger.PropertyParameter.DamageOverflow => Math.Max(GetDamage(rule) - GetHPBeforeDamage(rule), 0), 
			WarhammerKillTrigger.PropertyParameter.Penetration => Math.Max(GetPenetration(rule), 0), 
			_ => null, 
		};
	}

	private static int GetDamage(RulebookTargetEvent rule)
	{
		if (!(rule is RuleDealDamage { Result: var result }))
		{
			if (!(rule is RuleRollDamage { ResultValue: var resultValue }))
			{
				throw new NotImplementedException();
			}
			return resultValue;
		}
		return result;
	}

	private static int GetHPBeforeDamage(RulebookTargetEvent rule)
	{
		if (!(rule is RuleDealDamage { HPBeforeDamage: var hPBeforeDamage }))
		{
			if (rule is RuleRollDamage ruleRollDamage)
			{
				return ruleRollDamage.ConcreteTarget.GetHealthOptional()?.HitPointsLeft ?? 0;
			}
			throw new NotImplementedException();
		}
		return hPBeforeDamage;
	}

	private static int GetPenetration(IDamageHolderRule rule)
	{
		return rule.Damage.Penetration.Value;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
