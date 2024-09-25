using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("25c4a6865d92b6d4fac323b707b01471")]
public class AddInitiatorHealTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleHealDamage>, IRulebookHandler<RuleHealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IInitiatorRulebookHandler<RuleHealStatDamage>, IRulebookHandler<RuleHealStatDamage>, IHashable
{
	public ActionList Action;

	public ActionList HealerAction;

	public bool OnHealDamage;

	public bool OnHealStatDamage;

	public bool OnHealEnergyDrain;

	private void RunAction(RulebookEvent evt, MechanicEntity caster, MechanicEntity target)
	{
		base.Fact.RunActionInContext(Action);
		if (HealerAction.HasActions && evt.Reason.Context != null)
		{
			using (evt.Reason.Context.GetDataScope(caster.ToITargetWrapper()))
			{
				HealerAction.Run();
			}
		}
		if (Action.HasActions && evt.Reason.Context != null)
		{
			using (evt.Reason.Context.GetDataScope(target.ToITargetWrapper()))
			{
				Action.Run();
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
			RunAction(evt, evt.ConcreteInitiator, evt.ConcreteTarget);
		}
	}

	public void OnEventAboutToTrigger(RuleHealStatDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleHealStatDamage evt)
	{
		if (OnHealStatDamage && (evt.HealedDamage > 0 || evt.HealedDrain > 0))
		{
			RunAction(evt, evt.ConcreteInitiator, evt.Target);
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
