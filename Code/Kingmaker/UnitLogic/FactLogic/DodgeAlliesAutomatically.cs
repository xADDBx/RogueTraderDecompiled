using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("0bd3b0417d194307b76dfa6a66522267")]
public class DodgeAlliesAutomatically : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ITargetRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && evt.InitiatorUnit != null && evt.TargetUnit != null && evt.InitiatorUnit.IsAlly(evt.TargetUnit) && !evt.TargetUnit.Features.AutoDodge && !evt.TargetUnit.Features.AutoDodgeFriendlyFire)
		{
			evt.TargetUnit.Features.AutoDodgeFriendlyFire.Retain();
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && (bool)evt.TargetUnit?.Features.AutoDodgeFriendlyFire)
		{
			evt.TargetUnit?.Features.AutoDodgeFriendlyFire.Release();
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && evt.InitiatorUnit != null && evt.TargetUnit != null && evt.InitiatorUnit.IsAlly(evt.TargetUnit) && !evt.TargetUnit.Features.AutoDodge && !evt.TargetUnit.Features.AutoDodgeFriendlyFire)
		{
			evt.TargetUnit.Features.AutoDodgeFriendlyFire.Retain();
		}
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && (bool)evt.TargetUnit?.Features.AutoDodgeFriendlyFire)
		{
			evt.TargetUnit?.Features.AutoDodgeFriendlyFire.Release();
		}
	}

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			MechanicEntity maybeAttacker = evt.MaybeAttacker;
			if (maybeAttacker != null && maybeAttacker.IsAlly(evt.Defender) && !evt.Defender.Features.AutoDodge && !evt.Defender.Features.AutoDodgeFriendlyFire)
			{
				evt.Defender.Features.AutoDodgeFriendlyFire.Retain();
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability) && (bool)evt.Defender.Features.AutoDodgeFriendlyFire)
		{
			evt.Defender.Features.AutoDodgeFriendlyFire.Release();
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
