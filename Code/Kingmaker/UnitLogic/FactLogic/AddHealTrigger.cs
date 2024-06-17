using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("ebb2957e468e6594c9b7ae0005338984")]
public class AddHealTrigger : UnitFactComponentDelegate, ITargetRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleHealStatDamage>, IRulebookHandler<RuleHealStatDamage>, IHashable
{
	public ActionList Action;

	public ActionList HealerAction;

	public bool OnHealDamage;

	public bool OnHealStatDamage;

	public bool OnHealEnergyDrain;

	private void RunAction(RulebookEvent evt)
	{
		base.Fact.RunActionInContext(Action);
		if (HealerAction.HasActions && evt.Reason.Context != null)
		{
			using (evt.Reason.Context.GetDataScope(base.OwnerTargetWrapper))
			{
				HealerAction.Run();
			}
		}
	}

	public void OnEventAboutToTrigger(RuleHealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleHealDamage evt)
	{
		if (OnHealDamage && evt.Value > 0)
		{
			RunAction(evt);
		}
	}

	public void OnEventAboutToTrigger(RuleHealStatDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleHealStatDamage evt)
	{
		if (OnHealStatDamage && (evt.HealedDamage > 0 || evt.HealedDrain > 0))
		{
			RunAction(evt);
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
