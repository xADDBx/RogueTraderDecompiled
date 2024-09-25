using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("7d07d6649d9d471c82592b2b50405df1")]
public class ReceiveDamageInsteadOfCaster : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public int MaxDistanceToCaster;

	private Cells MaxDistanceToCasterCells => MaxDistanceToCaster.Cells();

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		if (maybeCaster != null && maybeCaster != base.Owner && maybeCaster == evt.Target && !maybeCaster.IsDeadOrUnconscious && !base.Owner.IsDeadOrUnconscious && base.Owner.DistanceToInCells(maybeCaster) <= MaxDistanceToCasterCells.Value)
		{
			evt.RollDamageRule.NullifyDamage(base.Fact);
			DamageData damage = evt.Damage.CopyWithoutModifiers();
			Rulebook.Trigger(new RuleDealDamage(evt.ConcreteInitiator, base.Owner, damage)
			{
				Reason = evt.Reason
			});
		}
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
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
