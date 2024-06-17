using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fc7d47b2586912942938f8a813b52e9b")]
public class WarhammerAbilityUsedAroundTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public ParticipantFaction caster;

	public ParticipantFaction target;

	public TargetSelection targetSelector;

	public bool limitByRange;

	[ShowIf("limitByRange")]
	public int minRange;

	[ShowIf("limitByRange")]
	public int maxRange;

	public bool checkForOverwatch;

	public ActionList Actions;

	private void RunAction(AbilityData _, MechanicEntity initiator, TargetWrapper target)
	{
		base.Fact.RunActionInContext(Actions, SelectTarget(initiator, target).ToITargetWrapper());
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public MechanicEntity SelectTarget(MechanicEntity initiator, TargetWrapper target)
	{
		if (targetSelector != 0)
		{
			return target.Entity;
		}
		return initiator;
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (!CheckFaction(evt.ConcreteInitiator, caster) || !CheckFaction(evt.SpellTarget.Entity, target))
		{
			return;
		}
		bool flag = false;
		if (limitByRange && base.Context.MaybeOwner != null)
		{
			int warhammerCellDistance = GeometryUtils.GetWarhammerCellDistance(base.Context.MaybeOwner.Position, SelectTarget(evt.ConcreteInitiator, evt.SpellTarget).Position, GraphParamsMechanicsCache.GridCellSize);
			if (warhammerCellDistance >= minRange || warhammerCellDistance <= maxRange)
			{
				flag = true;
			}
		}
		if ((!limitByRange || flag) && (!checkForOverwatch || evt.Spell.Blueprint.UsingInOverwatchArea == BlueprintAbility.UsingInOverwatchAreaType.WillCauseAttack))
		{
			RunAction(evt.Spell, evt.ConcreteInitiator, evt.SpellTarget);
		}
	}

	private bool CheckFaction(MechanicEntity entity, ParticipantFaction faction)
	{
		if (entity == null)
		{
			return false;
		}
		PartCombatGroup combatGroupOptional = entity.GetCombatGroupOptional();
		if (combatGroupOptional == null)
		{
			return false;
		}
		switch (faction)
		{
		case ParticipantFaction.Any:
			return true;
		case ParticipantFaction.Enemy:
			return combatGroupOptional.IsEnemy(base.Context.MaybeOwner);
		case ParticipantFaction.OtherAlly:
			if (entity != base.Context.MaybeOwner)
			{
				return combatGroupOptional.IsAlly(base.Context.MaybeOwner);
			}
			return false;
		case ParticipantFaction.AllyAndOwner:
			if (entity != base.Context.MaybeOwner)
			{
				return combatGroupOptional.IsAlly(base.Context.MaybeOwner);
			}
			return true;
		default:
			PFLog.Default.Error("WarhammerAbilityUsedAroundTrigger: unsupported ParticipantFaction type");
			return false;
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
