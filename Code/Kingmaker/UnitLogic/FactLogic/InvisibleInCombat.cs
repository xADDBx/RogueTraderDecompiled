using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("526148bffb304cceb280bcbfe1ab7479")]
public class InvisibleInCombat : UnitBuffComponentDelegate, IWarhammerAttackHandler, ISubscriber, IDamageHandler, IDirectMovementHandler, ISubscriber<IMechanicEntity>, IUnitRunCommandHandler, IUnitCommandEndHandler, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, ITurnStartHandler, ITurnEndHandler, IContinueTurnHandler, IInterruptCurrentTurnHandler, IInterruptTurnStartHandler, IInterruptTurnEndHandler, IInterruptTurnContinueHandler, IHashable
{
	[SerializeField]
	private int m_RevealRadius;

	[SerializeField]
	private RevealReason m_RevealReason;

	private UnitMovementAgentBase MaybeMovementAgent => base.Owner?.MovementAgent;

	protected override void OnActivateOrPostLoad()
	{
		PartUnitInvisibleInCombat orCreate = base.Owner.GetOrCreate<PartUnitInvisibleInCombat>();
		if (orCreate.SourceBuff != null && orCreate.SourceBuff != base.Buff)
		{
			PFLog.EntityFact.ErrorWithReport($"{base.Buff.Blueprint}: There is already a buff which provides invisibility: {orCreate.SourceBuff.Blueprint}. Removing {base.Buff.Blueprint}.");
			base.Buff.Remove();
			return;
		}
		orCreate.RevealRadius = m_RevealRadius;
		orCreate.RevealReason = m_RevealReason;
		orCreate.SourceBuff = base.Buff;
		base.Owner.GetOrCreate<PartUnitInvisible>().UseAttackOfOpportunity = true;
		base.Owner.GetOrCreate<PartCameraFollowTarget>().ForceIgnore = true;
	}

	protected override void OnDeactivate()
	{
		PartUnitInvisibleInCombat optional = base.Owner.GetOptional<PartUnitInvisibleInCombat>();
		if (optional?.SourceBuff == null || optional.SourceBuff == base.Buff)
		{
			SetGhosted(ghosted: false);
			base.Owner.Remove<PartUnitInvisible>();
			base.Owner.Remove<PartUnitInvisibleInCombat>();
			base.Owner.Remove<PartCameraFollowTarget>();
		}
	}

	void IWarhammerAttackHandler.HandleAttack(RulePerformAttack withWeaponAttackHit)
	{
		if (m_RevealReason.Has(RevealReason.Attack) && withWeaponAttackHit.Initiator == base.Owner)
		{
			base.Buff.Remove();
		}
	}

	void IDamageHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		if (m_RevealReason.Has(RevealReason.ReceiveDamage) && dealDamage.Target == base.Owner)
		{
			base.Buff.Remove();
		}
	}

	void IUnitRunCommandHandler.HandleUnitRunCommand(AbstractUnitCommand command)
	{
		UnitMoveToProper moveCommand = command as UnitMoveToProper;
		if (moveCommand != null)
		{
			moveCommand.Executor.ProcessMoveStart(moveCommand.ForcedPath, interruptPlayerMovement: true, delegate(ForcedPath newPath)
			{
				moveCommand.Params.ForcedPath = newPath;
			});
		}
	}

	void IUnitMoveHandler.HandleUnitMovement(AbstractUnitEntity unit)
	{
		unit.ProcessMoveTick();
	}

	void IUnitCommandEndHandler.HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper)
		{
			(command.Executor as BaseUnitEntity).ProcessMoveEnd();
		}
	}

	void IDirectMovementHandler.HandleDirectMovementStarted(ForcedPath path, bool _)
	{
		EventInvokerExtensions.BaseUnitEntity.ProcessMoveStart(path, interruptPlayerMovement: false);
	}

	void IDirectMovementHandler.HandleDirectMovementEnded()
	{
		EventInvokerExtensions.BaseUnitEntity.ProcessMoveEnd();
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		TrySetGhosted(ghosted: true);
	}

	void IContinueTurnHandler.HandleUnitContinueTurn(bool isTurnBased)
	{
		TrySetGhosted(ghosted: true);
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		TrySetGhosted(ghosted: false);
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		TrySetGhosted(ghosted: true);
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		TrySetGhosted(ghosted: true);
	}

	void IInterruptTurnEndHandler.HandleUnitEndInterruptTurn()
	{
		TrySetGhosted(ghosted: false);
	}

	void IInterruptCurrentTurnHandler.HandleOnInterruptCurrentTurn()
	{
		TrySetGhosted(ghosted: false);
	}

	private void TrySetGhosted(bool ghosted)
	{
		BaseUnitEntity baseUnitEntity = (BaseUnitEntity)ContextData<EventInvoker>.Current.InvokerEntity;
		if (baseUnitEntity != base.Owner && baseUnitEntity.IsEnemy(base.Owner))
		{
			SetGhosted(ghosted);
		}
	}

	private void SetGhosted(bool ghosted)
	{
		PartUnitInvisibleInCombat required = base.Owner.GetRequired<PartUnitInvisibleInCombat>();
		if (required.IsGhosted != ghosted)
		{
			required.IsGhosted = ghosted;
			if (ghosted)
			{
				base.Owner.Features.Hidden.Retain();
			}
			else
			{
				base.Owner.Features.Hidden.Release();
			}
			MaybeMovementAgent?.UpdateBlocker();
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
