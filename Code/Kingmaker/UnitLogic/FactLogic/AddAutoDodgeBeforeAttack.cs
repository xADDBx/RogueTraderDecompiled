using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("8d76c080808a4cfca3e4e6f96204777b")]
public class AddAutoDodgeBeforeAttack : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformDodge>, IRulebookHandler<RulePerformDodge>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool AutoDodge { get; set; }
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionsOnAutoDodgeAttacker;

	public ActionList ActionsOnAutoDodgeDefender;

	public void OnEventAboutToTrigger(RulePerformDodge evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			RequestTransientData<ComponentData>().AutoDodge = true;
			base.Owner.Features.AutoDodge.Retain();
		}
	}

	public void OnEventDidTrigger(RulePerformDodge evt)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.AutoDodge)
		{
			base.Owner.Features.AutoDodge.Release();
			componentData.AutoDodge = false;
			base.Fact.RunActionInContext(ActionsOnAutoDodgeAttacker, evt.Attacker.ToITargetWrapper());
			base.Fact.RunActionInContext(ActionsOnAutoDodgeDefender, evt.Defender.ToITargetWrapper());
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
