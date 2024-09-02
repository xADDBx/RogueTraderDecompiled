using System;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.Controllers.Combat;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;

namespace Kingmaker.Controllers.Units;

public class UnitCommandController : BaseUnitController
{
	public override TickType GetTickType()
	{
		return TickType.Simulation;
	}

	protected sealed override void TickOnUnit(AbstractUnitEntity unit)
	{
		if (unit.View == null || ((bool)unit.View.RigidbodyController && unit.View.RigidbodyController.IsControllingRigidbody) || unit.View.IsGetUp)
		{
			return;
		}
		try
		{
			using (ProfileScope.New("TickCommand"))
			{
				AbstractUnitCommand current = unit.Commands.Current;
				if (current != null)
				{
					TickCommand(current);
				}
			}
			unit.Commands.RemoveFinishedAndUpdateQueue();
			bool flag = unit.Commands.Empty;
			if (flag)
			{
				foreach (AbilityExecutionProcess ability in Game.Instance.AbilityExecutor.Abilities)
				{
					if (ability.Context.Caster == unit && ability.Context.Ability.Blueprint.IsMoveUnit)
					{
						flag = false;
					}
				}
			}
			if (flag)
			{
				unit.View.StopMoving();
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(unit.View, ex);
		}
	}

	protected override void AfterTick()
	{
		UnitCommandHandle.Cleanup();
	}

	private static void TickCommand([NotNull] AbstractUnitCommand command)
	{
		if (command.IsBlockingCommand && !Game.Instance.Player.IsBlockedOn(command))
		{
			return;
		}
		AbstractUnitEntity targetUnit = command.TargetUnit;
		if (command.ShouldBeInterrupted)
		{
			goto IL_005b;
		}
		if (targetUnit != null)
		{
			TargetWrapper target = command.Target;
			if ((object)target != null && !target.IsPoint && (!targetUnit.IsInState || AbstractUnitCommand.CommandTargetUntargetable(command.Executor, targetUnit)))
			{
				goto IL_005b;
			}
		}
		goto IL_0061;
		IL_005b:
		command.Interrupt();
		goto IL_0061;
		IL_0061:
		if (!command.IsStarted)
		{
			if (command.Params.InterruptAsSoonAsPossible)
			{
				command.Interrupt();
			}
			else
			{
				bool turnBasedModeActive = Game.Instance.TurnController.TurnBasedModeActive;
				if (ShouldStartCommand(command))
				{
					command.Start();
				}
				else if (!command.IsUnitEnoughClose || (turnBasedModeActive && !IsCommandWaitingForAnimation(command)))
				{
					command.Interrupt();
				}
			}
		}
		if (command.IsRunning)
		{
			if (command.ShouldTurnToTarget)
			{
				command.TurnToTarget();
			}
			using (ProfileScope.New("Tick"))
			{
				command.Tick();
			}
			PartUnitState stateOptional = command.Executor.GetStateOptional();
			if (stateOptional != null && !stateOptional.CanAct)
			{
				command.Executor.Commands.InterruptAllInterruptible();
			}
			else if (command.Params.InterruptAsSoonAsPossible && command.IsInterruptible)
			{
				command.Interrupt();
			}
		}
	}

	private static bool ShouldStartCommand(AbstractUnitCommand command)
	{
		AbstractUnitEntity executor = command.Executor;
		if (IsCommandWaitingForAnimation(command))
		{
			return false;
		}
		if (command.AwaitMovementFinish && executor.View.MovementAgent.IsReallyMoving)
		{
			return false;
		}
		if (!command.CanStart)
		{
			return false;
		}
		bool flag = !command.IsStarted && command.Result == AbstractUnitCommand.ResultType.None && command.IsUnitEnoughClose;
		if (flag && (command is UnitAreaTransition || command is UnitMoveTo))
		{
			return true;
		}
		PartUnitState stateOptional = executor.GetStateOptional();
		if ((stateOptional != null && !stateOptional.CanAct) || (executor.View is UnitEntityView unitEntityView && Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(unitEntityView.HandsEquipment)))
		{
			return false;
		}
		bool flag2 = !executor.IsInCombat || (executor.GetCombatStateOptional()?.CanActInCombat ?? false);
		return flag && flag2;
	}

	private static bool IsCommandWaitingForAnimation(AbstractUnitCommand command)
	{
		AbstractUnitEntity executor = command.Executor;
		UnitEntityView unitEntityView = executor.View as UnitEntityView;
		_ = unitEntityView != null;
		bool flag = unitEntityView != null && Game.Instance.HandsEquipmentController.IsUpdateScheduledFor(unitEntityView.HandsEquipment);
		bool flag2 = executor.AnimationManager != null && executor.AnimationManager.IsPreventingMovement;
		if (!((executor.AreHandsBusyWithAnimation && !command.DontWaitForHands) || flag))
		{
			return command.IsMoveUnit && flag2;
		}
		return true;
	}
}
