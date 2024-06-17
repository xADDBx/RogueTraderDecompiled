using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("8ac221330c6f48dba2d89cced59cd5d9")]
public class TutorialTriggerAttackOfOpportunity : TutorialTriggerRulebookEvent<RulePerformAttack>, IUnitMovementHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, ITurnStartHandler, ISubscriber<IMechanicEntity>, IHashable
{
	protected override bool ShouldTrigger(RulePerformAttack rule)
	{
		if (rule.ResultIsHit && rule.Initiator.IsPlayerFaction)
		{
			return rule.Ability.IsAttackOfOpportunity;
		}
		return false;
	}

	public void HandleMovementComplete()
	{
		TryToTrigger();
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		TryToTrigger();
	}

	private void TryToTrigger()
	{
		IEntity entity = ContextData<EventInvoker>.Current?.InvokerEntity;
		BaseUnitEntity unit = entity as BaseUnitEntity;
		if (unit != null && unit.IsPlayerFaction && unit.IsEngagedInMelee())
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}

	public void HandleWaypointUpdate(int index)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
