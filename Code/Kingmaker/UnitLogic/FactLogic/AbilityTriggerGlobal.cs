using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("f24c2d6da8b04755b2b1a5ebbee085ae")]
public class AbilityTriggerGlobal : AbilityTrigger, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool AssignOwnerAsTarget;

	public bool AssignCasterAsTarget;

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (!base.Owner.IsPreview && Restrictions.IsPassed(base.Fact, evt, evt.Spell))
		{
			BlueprintAbility ability = evt.Spell.Blueprint;
			MechanicEntity concreteInitiator = evt.ConcreteInitiator;
			TargetWrapper spellTarget = evt.SpellTarget;
			if ((concreteInitiator ?? spellTarget.Entity) == null)
			{
				PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
			}
			else if ((!ForOneAbility || ability == base.Ability) && (!ForMultipleAbilities || Abilities.HasItem((BlueprintAbilityReference r) => r.Is(ability))) && (!ForAbilityGroup || ability.AbilityGroups.Contains(base.AbilityGroup)))
			{
				base.Fact.RunActionInContext(Action, AssignOwnerAsTarget ? ((TargetWrapper)base.Owner) : (AssignCasterAsTarget ? ((TargetWrapper)concreteInitiator) : spellTarget));
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
