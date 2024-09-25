using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("cca012c5a4ea4b398881ef8b670cae44")]
public class GainMomentumForLostTemporaryWounds : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public int OldTempWounds { get; set; }
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth != null)
		{
			RequestTransientData<ComponentData>().OldTempWounds = evt.TargetHealth.TemporaryHitPoints;
		}
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth != null && evt.HPBeforeDamage > 0)
		{
			int temporaryHitPoints = evt.TargetHealth.TemporaryHitPoints;
			ComponentData componentData = RequestTransientData<ComponentData>();
			int num = componentData.OldTempWounds - temporaryHitPoints;
			componentData.OldTempWounds = 0;
			if (num > 0)
			{
				BaseUnitEntity owner = base.Owner;
				RuleReason reason = base.Context;
				Rulebook.Trigger(RulePerformMomentumChange.CreateCustom(owner, num, in reason));
			}
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
