using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
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

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("0cdbc172cfe945e3818c0d49fbd7d65f")]
public class TurnBasedModeEventsTrigger : UnitFactComponentDelegate, ITurnBasedModeHandler, ISubscriber, IRoundStartHandler, IRoundEndHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, ITurnEndHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, IHashable
{
	private class ComponentData : IEntityFactComponentTransientData
	{
		public bool WasParticipantInLastFight;
	}

	public bool TriggerIfNotInCombat;

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

	private bool IsInCorrectCombatState
	{
		get
		{
			if (!TriggerIfNotInCombat)
			{
				if (!base.Owner.IsInCombat)
				{
					if (base.Owner.IsPet)
					{
						return base.Owner.Master.IsInCombat;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!base.Owner.IsInGame || !CheckRestrictions() || base.Owner.IsPreviewUnit)
		{
			return;
		}
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (isTurnBased)
		{
			if (IsInCorrectCombatState)
			{
				componentData.WasParticipantInLastFight = true;
				using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
				{
					base.Fact.RunActionInContext(CombatStartActions, base.OwnerTargetWrapper);
					return;
				}
			}
			componentData.WasParticipantInLastFight = false;
		}
		else if (componentData.WasParticipantInLastFight)
		{
			using (base.Fact.MaybeContext?.GetDataScope(base.OwnerTargetWrapper))
			{
				base.Fact.RunActionInContext(CombatEndActions, base.OwnerTargetWrapper);
			}
		}
	}

	protected override void OnFactAttached()
	{
		if (!Game.Instance.TurnController.TurnBasedModeActive || base.Owner.IsPreviewUnit)
		{
			return;
		}
		using (base.Fact.MaybeContext?.GetDataScope(base.Owner.ToITargetWrapper()))
		{
			base.Fact.RunActionInContext(CombatStartActions, base.Owner.ToITargetWrapper());
		}
	}

	protected override void OnActivateOrPostLoad()
	{
		if (!base.Owner.IsPreviewUnit && IsInCorrectCombatState)
		{
			RequestTransientData<ComponentData>().WasParticipantInLastFight = true;
		}
	}

	void IRoundStartHandler.HandleRoundStart(bool isTurnBased)
	{
		HandleRoundEvent(isTurnBased, RoundStartActions);
	}

	void IRoundEndHandler.HandleRoundEnd(bool isTurnBased, bool isFirst)
	{
		if (!isFirst)
		{
			HandleRoundEvent(isTurnBased, RoundEndActions);
		}
	}

	private void HandleRoundEvent(bool isTurnBased, ActionList actionList)
	{
		if (isTurnBased && !base.Owner.IsPreviewUnit && IsInCorrectCombatState && CheckRestrictions())
		{
			base.Fact.RunActionInContext(actionList, base.OwnerTargetWrapper);
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			HandleUnitTurnEvent(UnitTurnStartActions);
		}
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			HandleUnitTurnEvent(UnitTurnEndActions);
		}
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (!DoNotApplyOnInterrupts || interruptionData.AsExtraTurn)
		{
			HandleUnitTurnEvent(UnitInterruptTurnStartActions);
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		HandleUnitTurnEvent(UnitInterruptTurnEndActions);
	}

	private void HandleUnitTurnEvent(ActionList actionList)
	{
		if (base.Owner.IsPreviewUnit || !IsInCorrectCombatState)
		{
			return;
		}
		MechanicEntity mechanicEntity = EventInvokerExtensions.MechanicEntity;
		if (!((ActionsOnTheTurnOwner && mechanicEntity is UnitEntity unitEntity) ? unitEntity : base.Owner).IsPreviewUnit && (AnyUnitTurns || mechanicEntity == base.Owner) && (!OnlyEnemyTurns || !base.Owner.IsAlly(mechanicEntity)) && CheckRestrictions())
		{
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
			base.Fact.RunActionInContext(actionList, target);
		}
	}

	private bool CheckRestrictions()
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			return Restrictions.IsPassed(base.Fact, base.Owner);
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
