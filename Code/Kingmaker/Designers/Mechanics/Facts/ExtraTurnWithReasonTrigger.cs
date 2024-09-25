using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("6a1959011ccce504cb1b80db926ed4cc")]
public class ExtraTurnWithReasonTrigger : UnitFactComponentDelegate, IInterruptCurrentTurnHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool AnyUnitTurns;

	[ShowIf("AnyUnitTurns")]
	public bool OnlyEnemyTurns;

	public bool ActionsOnTheTurnOwner;

	public ActionList UnitInterruptTurnStartActions;

	public void HandleOnInterruptCurrentTurn()
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!Game.Instance.Player.IsInCombat || (mechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(mechanicEntity) && OnlyEnemyTurns))
		{
			return;
		}
		ITargetWrapper targetWrapper;
		if (!ActionsOnTheTurnOwner || !(mechanicEntity is UnitEntity entity))
		{
			targetWrapper = base.OwnerTargetWrapper;
		}
		else
		{
			ITargetWrapper targetWrapper2 = entity.ToTargetWrapper();
			targetWrapper = targetWrapper2;
		}
		ITargetWrapper target = targetWrapper;
		using (base.Fact.MaybeContext?.GetDataScope(target))
		{
			base.Fact.RunActionInContext(UnitInterruptTurnStartActions, target);
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
