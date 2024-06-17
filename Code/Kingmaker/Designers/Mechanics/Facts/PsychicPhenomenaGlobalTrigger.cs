using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("635dc8ac062d4b6b911d8a84c7ce7832")]
public class PsychicPhenomenaGlobalTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, ISubscriber, IGlobalRulebookSubscriber, IFakePsychicPhenomenaTrigger, ISubscriber<IBaseUnitEntity>, IHashable
{
	public ActionList Actions;

	public ActionList OnlyPhenomenaActions;

	public ActionList OnlyPerilsActions;

	public void OnEventAboutToTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
	}

	public void OnEventDidTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
		NewMethod(evt.IsPsychicPhenomena, evt.IsPerilsOfTheWarp);
	}

	private void NewMethod(bool isPsychicPhenomena, bool isPerilsOfTheWarp)
	{
		if (isPsychicPhenomena)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
			{
				base.Fact.RunActionInContext(Actions, base.Owner.ToITargetWrapper());
				base.Fact.RunActionInContext(OnlyPhenomenaActions, base.Owner.ToITargetWrapper());
			}
		}
		if (isPerilsOfTheWarp)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
			{
				base.Fact.RunActionInContext(Actions, base.Owner.ToITargetWrapper());
				base.Fact.RunActionInContext(OnlyPerilsActions, base.Owner.ToITargetWrapper());
			}
		}
	}

	public void HandleFakePsychicPhenomena(bool isPsychicPhenomena, bool isPerilsOfTheWarp)
	{
		NewMethod(isPsychicPhenomena, isPerilsOfTheWarp);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
