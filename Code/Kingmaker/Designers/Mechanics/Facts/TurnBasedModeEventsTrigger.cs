using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
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
[TypeId("0cdbc172cfe945e3818c0d49fbd7d65f")]
public class TurnBasedModeEventsTrigger : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IRoundStartHandler, IRoundEndHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList CombatStartActions;

	public ActionList CombatEndActions;

	public ActionList RoundStartActions;

	public ActionList RoundEndActions;

	public bool AnyUnitTurns;

	[ShowIf("AnyUnitTurns")]
	public bool OnlyEnemyTurns;

	public bool ActionsOnTheTurnOwner;

	[Space(4f)]
	public ActionList UnitTurnStartActions;

	public ActionList UnitTurnEndActions;

	[Space(4f)]
	[InspectorName("AdditionalTurnStart actions")]
	public ActionList UnitInterruptTurnStartActions;

	[InspectorName("AdditionalTurnEnd actions")]
	public ActionList UnitInterruptTurnEndActions;

	[Space(4f)]
	public bool DoNotApplyOnInterrupts;

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		if (isTurnBased)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(CombatStartActions, base.OwnerTargetWrapper);
				return;
			}
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
		{
			base.Fact.RunActionInContext(CombatEndActions, base.OwnerTargetWrapper);
		}
	}

	protected override void OnFactAttached()
	{
		if (!Game.Instance.TurnController.TurnBasedModeActive)
		{
			return;
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(CombatStartActions, base.Owner.ToITargetWrapper());
		}
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		if (isTurnBased)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(RoundStartActions, base.OwnerTargetWrapper);
			}
		}
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		if (isTurnBased)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(RoundEndActions, base.OwnerTargetWrapper);
			}
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!isTurnBased || (mechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(mechanicEntity) && OnlyEnemyTurns))
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
			base.Fact.RunActionInContext(UnitTurnStartActions, target);
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!isTurnBased || (EventInvokerExtensions.MechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(EventInvokerExtensions.MechanicEntity) && OnlyEnemyTurns))
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
			base.Fact.RunActionInContext(UnitTurnEndActions, target);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (DoNotApplyOnInterrupts && !interruptionData.AsExtraTurn)
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if ((EventInvokerExtensions.MechanicEntity != base.Owner && !AnyUnitTurns) || (base.Owner.IsAlly(EventInvokerExtensions.MechanicEntity) && OnlyEnemyTurns))
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

	public void HandleUnitEndInterruptTurn()
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, base.Owner))
			{
				return;
			}
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (EventInvokerExtensions.MechanicEntity != base.Owner && !AnyUnitTurns)
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
			base.Fact.RunActionInContext(UnitInterruptTurnEndActions, target);
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
