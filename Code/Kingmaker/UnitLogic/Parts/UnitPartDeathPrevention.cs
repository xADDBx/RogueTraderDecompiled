using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartDeathPrevention : BaseUnitPart, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleDealStatDamage>, IRulebookHandler<RuleDealStatDamage>, IHashable
{
	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (base.Owner.Health.Damage >= (int)base.Owner.Health.HitPoints)
		{
			base.Owner.Health.SetHitPointsLeft(1);
		}
	}

	public void OnEventAboutToTrigger(RuleDealStatDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealStatDamage evt)
	{
		if (evt.Stat.ModifiedValueRaw < 1)
		{
			if (evt.IsDrain)
			{
				evt.Stat.Drain += 1 - evt.Stat.ModifiedValueRaw;
			}
			else
			{
				evt.Stat.Damage += 1 - evt.Stat.ModifiedValueRaw;
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
