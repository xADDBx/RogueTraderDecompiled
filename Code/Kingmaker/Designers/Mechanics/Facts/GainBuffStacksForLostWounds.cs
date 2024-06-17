using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dcff3d88d3404bb992eeabf649419b98")]
public class GainBuffStacksForLostWounds : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public int Div = 1;

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BuffEndCondition BuffEndCondition = BuffEndCondition.CombatEnd;

	public bool Permanent;

	[ShowIf("IsCustomDuration")]
	public ContextDurationValue DurationValue;

	public BlueprintBuff Buff => m_Buff?.Get();

	private bool IsCustomDuration => !Permanent;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.TargetHealth == null || evt.HPBeforeDamage <= 0 || !base.Owner.IsInCombat)
		{
			return;
		}
		int num = evt.Result / Div;
		if (num > 0)
		{
			BuffDuration duration = new BuffDuration(Permanent ? null : new Rounds?(DurationValue.Calculate(base.Context)), BuffEndCondition);
			Buff buff = base.Owner.Buffs.Add(Buff, base.Context, duration);
			if (num > 1)
			{
				buff?.AddRank(num - 1);
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
