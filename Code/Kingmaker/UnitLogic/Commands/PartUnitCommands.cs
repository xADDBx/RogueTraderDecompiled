using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Commands;

public sealed class PartUnitCommands : EntityPart<AbstractUnitEntity>, IUnitConditionsChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitCommands>, IEntityPartOwner
	{
		PartUnitCommands Commands { get; }
	}

	private readonly LinkedList<UnitCommandParams> m_Queue = new LinkedList<UnitCommandParams>();

	[CanBeNull]
	private AbstractUnitCommand m_Current;

	[CanBeNull]
	public GroupCommand GroupCommand => (m_Current as UnitGroupCommand)?.GroupCommand;

	[CanBeNull]
	public UnitMoveTo CurrentMoveTo => m_Current as UnitMoveTo;

	[CanBeNull]
	public UnitMoveContinuously CurrentMoveContinuously => m_Current as UnitMoveContinuously;

	[CanBeNull]
	public AbstractUnitCommand Current => m_Current;

	public LinkedList<UnitCommandParams> Queue => m_Queue;

	public bool Empty => m_Current == null;

	public bool HasAiCommand => false;

	public bool PreventMovement => m_Current?.IsPreventMovement ?? false;

	public bool Uninterruptible
	{
		get
		{
			AbstractUnitCommand current = m_Current;
			if (current != null)
			{
				return !current.IsInterruptible;
			}
			return false;
		}
	}

	public bool Contains(AbstractUnitCommand command)
	{
		return m_Current == command;
	}

	public bool Contains(UnitCommandHandle commandHandle)
	{
		return m_Current == commandHandle.Cmd;
	}

	public bool Contains(Func<AbstractUnitCommand, bool> pred)
	{
		if (m_Current != null)
		{
			return pred(m_Current);
		}
		return false;
	}

	public void InterruptAiCommands()
	{
	}

	[CanBeNull]
	public UnitCommandHandle Run(UnitCommandParams cmdParams)
	{
		if (!CanRun(cmdParams))
		{
			cmdParams.ForcedPath?.Release(cmdParams);
			return null;
		}
		if (!Game.Instance.UnitCommandBuffer.TryAdd(base.Owner, cmdParams))
		{
			return RunImmediate(cmdParams);
		}
		return UnitCommandHandle.Request(cmdParams);
	}

	[CanBeNull]
	public UnitCommandHandle RunImmediate(UnitCommandParams cmdParams)
	{
		if (!CanRun(cmdParams))
		{
			return null;
		}
		return RunInternal(cmdParams, fromQueue: false);
	}

	public bool CanRun([NotNull] UnitCommandParams cmdParams)
	{
		if (base.Owner.LifeState.IsConscious)
		{
			return cmdParams.IsDirectionCorrect;
		}
		return false;
	}

	[NotNull]
	private UnitCommandHandle RunInternal(UnitCommandParams cmdParams, bool fromQueue, bool doNotClearQueue = false)
	{
		using (ProfileScope.New("UnitCommands.Run"))
		{
			if (!fromQueue && !doNotClearQueue)
			{
				ClearQueue();
			}
			AbstractUnitCommand current = m_Current;
			if (current != null && current.IsRunning && cmdParams.TryMergeInto(m_Current))
			{
				m_Current.Params.InterruptAsSoonAsPossible = cmdParams.InterruptAsSoonAsPossible;
				UpdateCombatTarget(m_Current);
				return UnitCommandHandle.Request(m_Current);
			}
			if (!fromQueue && TryAddToQueueInsteadOfRunImmediately(cmdParams))
			{
				return UnitCommandHandle.Request(cmdParams);
			}
			ClearCurrent(interrupt: true);
			AbstractUnitCommand cmd = (m_Current = cmdParams.CreateCommand());
			cmd.Init(base.Owner);
			base.Owner.HoldState = false;
			UpdateCombatTarget(cmd);
			cmd.OnRun();
			EventBus.RaiseEvent(delegate(IUnitRunCommandHandler h)
			{
				h.HandleUnitRunCommand(cmd);
			});
			if (base.Owner.IsInCombat)
			{
				Game.Instance.PlayerInputInCombatController.RequestLockPlayerInput();
			}
			return UnitCommandHandle.Request(m_Current);
		}
	}

	private static void UpdateCombatTarget(AbstractUnitCommand cmd)
	{
		if (!(cmd.Executor is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		MechanicEntity mechanicEntity = cmd.Target?.Entity;
		if (mechanicEntity != null && mechanicEntity != cmd.Executor)
		{
			PartLifeState lifeStateOptional = mechanicEntity.GetLifeStateOptional();
			if (((lifeStateOptional != null && lifeStateOptional.IsConscious) || !baseUnitEntity.Brain.IsAIEnabled) && baseUnitEntity.CombatGroup.CanAttack(mechanicEntity))
			{
				baseUnitEntity.CombatState.LastTarget = mechanicEntity;
				if (cmd.MarkManualTarget)
				{
					baseUnitEntity.CombatState.ManualTarget = mechanicEntity;
				}
				else if (baseUnitEntity.CombatState.ManualTarget != mechanicEntity)
				{
					baseUnitEntity.CombatState.ManualTarget = null;
				}
				return;
			}
		}
		baseUnitEntity.CombatState.ManualTarget = null;
	}

	private UnitCommandHandle AddToQueueInternal(UnitCommandParams cmd, bool first)
	{
		if (base.Owner.IsInCombat)
		{
			Game.Instance.PlayerInputInCombatController.RequestLockPlayerInput();
		}
		if (first)
		{
			m_Queue.AddFirst(cmd);
		}
		else
		{
			m_Queue.AddLast(cmd);
		}
		return UnitCommandHandle.Request(cmd);
	}

	private UnitCommandHandle AddToQueueOrRun(UnitCommandParams cmdParams, bool first)
	{
		if (m_Current == null)
		{
			return RunInternal(cmdParams, fromQueue: false);
		}
		return AddToQueueInternal(cmdParams, first);
	}

	public UnitCommandHandle AddToQueue(UnitCommandParams cmdParams)
	{
		if (Game.Instance.UnitCommandBuffer.TryAddToQueue(base.Owner, cmdParams))
		{
			return UnitCommandHandle.Request(cmdParams);
		}
		return ForceAddToQueue(cmdParams);
	}

	public UnitCommandHandle ForceAddToQueue(UnitCommandParams cmd)
	{
		return AddToQueueOrRun(cmd, first: false);
	}

	public UnitCommandHandle AddToQueueFirst(UnitCommandParams cmd)
	{
		if (Game.Instance.UnitCommandBuffer.TryAddToQueueFirst(base.Owner, cmd))
		{
			return UnitCommandHandle.Request(cmd);
		}
		return ForceAddToQueueFirst(cmd);
	}

	public UnitCommandHandle ForceAddToQueueFirst(UnitCommandParams cmd)
	{
		return AddToQueueOrRun(cmd, first: true);
	}

	private bool TryAddToQueueInsteadOfRunImmediately(UnitCommandParams next)
	{
		AbstractUnitCommand current = m_Current;
		if (current != null && !current.IsInterruptible)
		{
			m_Current.Params.InterruptAsSoonAsPossible = true;
			AddToQueueInternal(next, first: false);
			return true;
		}
		if (m_Current is UnitMoveTo || next is UnitMoveToParams)
		{
			return false;
		}
		current = m_Current;
		int num;
		if (current != null && current.IsRunning && object.Equals(m_Current.Target, next.Target))
		{
			num = (next.IsUnitEnoughClose(base.Owner) ? 1 : 0);
			if (num != 0)
			{
				m_Current.Params.InterruptAsSoonAsPossible = true;
				AddToQueueInternal(next, first: false);
			}
		}
		else
		{
			num = 0;
		}
		return (byte)num != 0;
	}

	private void ClearCommand([CanBeNull] AbstractUnitCommand cmd, bool interrupt)
	{
		if (cmd?.Executor != null)
		{
			if (interrupt)
			{
				cmd.Interrupt();
			}
			cmd.Clear();
		}
	}

	private void ClearCurrent(bool interrupt)
	{
		ClearCommand(m_Current, interrupt);
		m_Current = null;
		if (base.Owner == null || base.Owner.IsInCombat)
		{
			Game.Instance.PlayerInputInCombatController.RequestUnlockPlayerInput();
		}
	}

	private void ClearQueue()
	{
		m_Queue.Clear();
		Game.Instance.PlayerInputInCombatController.RequestUpdate();
	}

	public void RemoveFinishedAndUpdateQueue()
	{
		using (ProfileScope.New("UnitCommands.RemoveFinishedAndUpdateQueue"))
		{
			AbstractUnitCommand current = m_Current;
			if ((current != null && current.IsFinished && !(current is UnitGroupCommand { GroupCommand: { IsFinished: false } })) ? true : false)
			{
				ClearCurrent(interrupt: false);
			}
			UnitCommandParams unitCommandParams = m_Queue.First?.Value;
			if (unitCommandParams != null && m_Current == null)
			{
				m_Queue.RemoveFirst();
				if (base.Owner == null || base.Owner.IsInCombat)
				{
					Game.Instance.PlayerInputInCombatController.RequestUnlockPlayerInput();
				}
				RunInternal(unitCommandParams, fromQueue: true);
			}
		}
	}

	public void InterruptAll(Func<AbstractUnitCommand, bool> pred)
	{
		if (m_Current != null && pred(m_Current))
		{
			ClearCurrent(interrupt: true);
		}
		ClearQueue();
	}

	public bool InterruptAllInterruptible()
	{
		bool flag = false;
		AbstractUnitCommand current = m_Current;
		if (current != null && !current.IsFinished)
		{
			if (!m_Current.IsInterruptible || m_Current.DoNotInterruptAfterFight)
			{
				goto IL_0062;
			}
			if (m_Current.FromCutscene && TurnController.IsInTurnBasedCombat())
			{
				AbstractUnitEntity owner = base.Owner;
				if (owner == null || owner.IsInCombat)
				{
					goto IL_0062;
				}
			}
			ClearCurrent(interrupt: true);
		}
		goto IL_0064;
		IL_0064:
		if (!flag)
		{
			ClearQueue();
		}
		return !flag;
		IL_0062:
		flag = true;
		goto IL_0064;
	}

	public void InterruptMove(bool byPlayer = false)
	{
		if (byPlayer)
		{
			Game.Instance.GameCommandQueue.InterruptMoveUnit(base.Owner);
		}
		else
		{
			ForceInterruptMove();
		}
	}

	public void ForceInterruptMove()
	{
		AbstractUnitCommand current = m_Current;
		if (current != null && current.IsMoveUnit)
		{
			ClearCurrent(interrupt: true);
		}
	}

	public void InterruptGroupCommand()
	{
		if (m_Current is UnitGroupCommand)
		{
			ClearCurrent(interrupt: true);
		}
	}

	[CanBeNull]
	public T GetCurrent<T>() where T : AbstractUnitCommand
	{
		return m_Current as T;
	}

	[CanBeNull]
	public T GetCurrentOrQueued<T>() where T : AbstractUnitCommand
	{
		return (m_Current as T) ?? m_Queue.OfType<T>().FirstOrDefault();
	}

	public IEnumerator<AbstractUnitCommand> GetEnumerator()
	{
		if (m_Current != null)
		{
			yield return m_Current;
		}
	}

	public bool IsRunning()
	{
		return m_Current?.IsRunning ?? false;
	}

	public bool HasOffensiveCommand()
	{
		if (m_Current != null && m_Current.Params.IsOffensiveCommand(base.Owner))
		{
			return true;
		}
		foreach (UnitCommandParams item in Queue)
		{
			if (item.IsOffensiveCommand(base.Owner))
			{
				return true;
			}
		}
		return false;
	}

	protected override void OnPostLoad()
	{
		Current?.PostLoad(base.Owner);
	}

	public void HandleUnitConditionsChanged(UnitCondition condition)
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (condition == UnitCondition.Prone && baseUnitEntity == base.ConcreteOwner && baseUnitEntity.State.HasCondition(condition))
		{
			InterruptMove();
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
