using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Optimization;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Serializable]
[TypeId("2e7912a03ad14212bba0291d847e1b7c")]
public class PsychicPhenomenaRedirect : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public ContextValue MaxRangeToRedirect;

	public RestrictionCalculator CasterRestrictions = new RestrictionCalculator();

	public RestrictionCalculator TargetRestrictions = new RestrictionCalculator();

	public bool RedirectToOwnerIfNoValidTarget;

	void IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>.OnEventAboutToTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
	}

	void IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>.OnEventDidTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
		if ((!evt.IsPsychicPhenomena && !evt.IsPerilsOfTheWarp) || !CasterRestrictions.IsPassed(base.Fact, evt.ConcreteInitiator))
		{
			return;
		}
		int num = MaxRangeToRedirect.Calculate(base.Context);
		List<BaseUnitEntity> list = EntityBoundsHelper.FindUnitsInRange(base.Owner.Position, num);
		list.RemoveAll((BaseUnitEntity i) => !TargetRestrictions.IsPassed(base.Fact, i));
		if (list.Empty())
		{
			if (RedirectToOwnerIfNoValidTarget)
			{
				evt.OverrideTarget = (base.Owner, base.Fact);
			}
		}
		else
		{
			evt.OverrideTarget = (list.Random(PFStatefulRandom.Mechanics), base.Fact);
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
