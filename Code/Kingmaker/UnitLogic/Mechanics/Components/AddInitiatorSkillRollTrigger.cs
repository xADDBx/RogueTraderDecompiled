using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[AllowMultipleComponents]
[TypeId("4e6fe9e1395573f458bafaa76364f65d")]
public class AddInitiatorSkillRollTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSkillCheck>, IRulebookHandler<RulePerformSkillCheck>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public bool OnlySuccess;

	public StatType Skill;

	public ActionList Action;

	public void OnEventAboutToTrigger(RulePerformSkillCheck evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSkillCheck evt)
	{
		if (CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
		}
	}

	private bool CheckConditions(RulePerformSkillCheck evt)
	{
		if (OnlySuccess && !evt.ResultIsSuccess)
		{
			return false;
		}
		if (Skill != 0 && Skill != evt.StatType)
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
