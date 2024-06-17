using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[TypeId("e0ecefa49eeb4f80a63dba55e4f9dfd8")]
public abstract class WarhammerDamageTrigger : UnitFactComponentDelegate, IHashable
{
	private static readonly HashSet<EntityFactComponent> TriggeringNow = new HashSet<EntityFactComponent>();

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool TriggersForDamageOverTime;

	protected void TryTrigger(RuleDealDamage rule)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, rule, rule.SourceAbility))
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

	protected abstract void OnTrigger(RuleDealDamage rule);

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
