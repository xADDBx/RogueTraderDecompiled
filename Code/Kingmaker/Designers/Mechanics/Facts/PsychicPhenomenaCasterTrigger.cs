using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("b584e6818b924d83b4299130eea4e552")]
public class PsychicPhenomenaCasterTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ActionList Actions;

	public ActionList OnlyPhenomenaActions;

	public ActionList OnlyPerilsActions;

	public void OnEventAboutToTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
		if (evt.IsPsychicPhenomena)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
			{
				base.Fact.RunActionInContext(Actions, base.Owner.ToITargetWrapper());
				base.Fact.RunActionInContext(OnlyPhenomenaActions, base.Owner.ToITargetWrapper());
			}
		}
		if (evt.IsPerilsOfTheWarp)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
			{
				base.Fact.RunActionInContext(Actions, base.Owner.ToITargetWrapper());
				base.Fact.RunActionInContext(OnlyPerilsActions, base.Owner.ToITargetWrapper());
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
