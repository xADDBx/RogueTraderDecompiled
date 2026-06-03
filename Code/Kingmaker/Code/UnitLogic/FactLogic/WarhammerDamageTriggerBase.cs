using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[TypeId("bbd12e2ec25d412496d3102c92e92a57")]
public abstract class WarhammerDamageTriggerBase : UnitFactComponentDelegate, IHashable
{
	private static readonly HashSet<EntityFactComponent> TriggeringNow = new HashSet<EntityFactComponent>();

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool TriggersForDamageOverTime;

	protected void TryTrigger<TEvent>(TEvent rule) where TEvent : RulebookTargetEvent, IDamageHolderRule
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			AbilityData ability = ((rule is RuleDealDamage ruleDealDamage) ? ruleDealDamage.SourceAbility : null);
			if (!Restrictions.IsPassed(base.Fact, rule, ability))
			{
				return;
			}
		}
		BlueprintScriptableObject blueprintScriptableObject = rule.Reason.Context?.AssociatedBlueprint;
		if ((!(blueprintScriptableObject is BlueprintBuff) && !(blueprintScriptableObject is BlueprintAbilityAreaEffect)) || TriggersForDamageOverTime)
		{
			if (TriggeringNow.Contains(base.Runtime))
			{
				throw new Exception($"Cycled trigger: {base.Fact}.{name}");
			}
			try
			{
				TriggeringNow.Add(base.Runtime);
				OnTrigger(rule);
			}
			finally
			{
				TriggeringNow.Remove(base.Runtime);
			}
			base.ExecutesCount++;
		}
	}

	protected abstract void OnTrigger<TEvent>(TEvent rule) where TEvent : RulebookTargetEvent, IDamageHolderRule;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
