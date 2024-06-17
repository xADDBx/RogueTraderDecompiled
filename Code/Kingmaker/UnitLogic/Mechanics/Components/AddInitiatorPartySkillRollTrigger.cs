using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[TypeId("51410a690caf66c44859c10594aa16b1")]
public class AddInitiatorPartySkillRollTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformPartySkillCheck>, IRulebookHandler<RulePerformPartySkillCheck>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool OnlySuccess;

	public StatType Skill;

	public ActionList Action;

	public void OnEventAboutToTrigger(RulePerformPartySkillCheck evt)
	{
	}

	public void OnEventDidTrigger(RulePerformPartySkillCheck evt)
	{
		if (evt.Roller == base.Owner && CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
		}
	}

	private bool CheckConditions(RulePerformPartySkillCheck evt)
	{
		if (OnlySuccess && !evt.Success)
		{
			return false;
		}
		if (Skill != evt.StatType)
		{
			return false;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
