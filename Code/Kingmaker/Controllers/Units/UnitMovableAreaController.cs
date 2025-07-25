using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UniRx;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitMovableAreaController : IControllerDisable, IController, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IContinueTurnHandler, IInterruptTurnStartHandler, IUnitCommandStartHandler, IUnitCommandEndHandler, IUnitCommandActHandler, IUnitActionPointsHandler, IUnitSpentMovementPoints, IUnitGainMovementPoints, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, INetRoleSetHandler, IDirectMovementHandler, IUnitGetAbilityJump, ISubscriber<IBaseUnitEntity>, IUnitMoveToProperHandler, IAbilityExecutionProcessHandler, IAbilityExecutionProcessClearedHandler, IInterruptTurnContinueHandler, ITurnEndHandler, IInterruptTurnEndHandler, IPlayerInputLockHandler
{
	private BaseUnitEntity m_CurrentUnit;

	private bool m_DeploymentPhase;

	private IDisposable m_CurrentUnitSubscription;

	private Dictionary<string, Vector3> m_InitialPositions = new Dictionary<string, Vector3>();

	public BaseUnitEntity CurrentUnit => m_CurrentUnit;

	public bool DeploymentPhase => m_DeploymentPhase;

	public List<GraphNode> CurrentUnitMovableArea { get; private set; }

	public List<GraphNode> DeploymentForbiddenArea { get; private set; }

	public void OnDisable()
	{
		Clear();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (ShouldHandle(command.Executor) && (command is UnitMoveToProper || command is UnitUseAbility || command.Executor.IsBusy))
		{
			HideMovableArea();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility) && !command.Executor.IsBusy)
		{
			UpdateMovableAreaIfNeeded(command.Executor);
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (ShouldHandle(context.MaybeCaster as AbstractUnitEntity))
		{
			HideMovableArea();
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
	}

	public void HandleExecutionProcessCleared(AbilityExecutionContext context)
	{
		if (ShouldHandle(context.MaybeCaster as AbstractUnitEntity))
		{
			MechanicEntity maybeCaster = context.MaybeCaster;
			if (maybeCaster == null || !maybeCaster.IsBusy)
			{
				UpdateMovableAreaIfNeeded(context.MaybeCaster as AbstractUnitEntity);
			}
		}
	}

	void IUnitMoveToProperHandler.HandleUnitMoveToProper(UnitMoveToProper cmd)
	{
		if (cmd != null && !cmd.FromCutscene && Game.Instance.TurnController.TurnBasedModeActive && cmd.Executor != m_CurrentUnit && m_CurrentUnit != null)
		{
			UpdateMovableArea();
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		UpdateMovableAreaIfNeeded(command.Executor);
	}

	public void HandleRestoreActionPoints()
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleDirectMovementStarted(ForcedPath path, bool disableAttacksOfOpportunity)
	{
	}

	public void HandleDirectMovementEnded()
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleActionPointsSpent(BaseUnitEntity unit)
	{
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	public void HandleUnitGainMovementPoints(float movementPoints, MechanicsContext context)
	{
		UpdateMovableAreaIfNeeded(EventInvokerExtensions.BaseUnitEntity);
	}

	private void UpdateMovableAreaIfNeeded(AbstractUnitEntity unit)
	{
		if (ShouldHandle(unit))
		{
			UpdateMovableArea();
		}
	}

	private bool ShouldHandle(AbstractUnitEntity unitEntity)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			return unitEntity == m_CurrentUnit;
		}
		return false;
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		if (!interruptionData.InterruptionWithoutInitiativeAndPanelUpdate)
		{
			HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
		}
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	private void HandleNewUnitStartTurn(MechanicEntity entity)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			Clear();
			UpdateMovableAreaForParty();
			MechanicEntity mechanicEntity = (Game.Instance.TurnController.IsPreparationTurn ? (entity ?? Game.Instance.TurnController.CurrentUnit) : (Game.Instance.TurnController.CurrentUnit ?? entity));
			if (mechanicEntity is BaseUnitEntity baseUnitEntity && (baseUnitEntity.IsPet || mechanicEntity.IsDirectlyControllable))
			{
				m_CurrentUnit = baseUnitEntity;
				UpdateMovableArea();
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			Clear();
			ClearInitialPositions();
		}
	}

	private void UpdateMovableAreaForParty()
	{
		foreach (BaseUnitEntity partyAndPet in Game.Instance.Player.PartyAndPets)
		{
			if (partyAndPet != null && !m_InitialPositions.ContainsKey(partyAndPet.UniqueId))
			{
				m_InitialPositions.Add(partyAndPet.UniqueId, partyAndPet.Position);
			}
		}
	}

	private void UpdateMovableArea()
	{
		Vector3 vector;
		if (m_DeploymentPhase)
		{
			if (m_InitialPositions.TryGetValue(m_CurrentUnit.UniqueId, out var value))
			{
				vector = value;
			}
			else
			{
				vector = m_CurrentUnit.Position;
				m_InitialPositions[m_CurrentUnit.UniqueId] = vector;
			}
		}
		else
		{
			vector = m_CurrentUnit.Position;
		}
		CurrentUnitMovableArea = GetMovableArea(m_CurrentUnit, vector);
		bool flag = m_CurrentUnit.HasMechanicFeature(MechanicsFeatureType.CanDeployNearEnemy);
		DeploymentForbiddenArea = ((!m_DeploymentPhase) ? null : (flag ? new List<GraphNode>() : GetDeploymentForbiddenArea().ToList()));
		EventBus.RaiseEvent((IMechanicEntity)m_CurrentUnit, (Action<IUnitMovableAreaHandler>)delegate(IUnitMovableAreaHandler h)
		{
			h.HandleSetUnitMovableArea(CurrentUnitMovableArea);
		}, isCheckRuntime: true);
	}

	private List<GraphNode> GetMovableArea(BaseUnitEntity unit, Vector3 position)
	{
		float num = (unit.IsPet ? 2f : (unit.HasMechanicFeature(MechanicsFeatureType.CantMove) ? 0f : unit.CombatState.ActionPointsBlue));
		if (!(num > 0f))
		{
			return null;
		}
		return PathfindingService.Instance.FindAllReachableTiles_Blocking(unit.View.MovementAgent, unit.IsPet ? unit.Master.Position : position, num).Keys.ToList();
	}

	private static IEnumerable<GraphNode> GetDeploymentForbiddenArea()
	{
		if (Game.Instance.CurrentMode == GameModeType.SpaceCombat)
		{
			return Enumerable.Empty<GraphNode>();
		}
		HashSet<GraphNode> hashSet = TempHashSet.Get<GraphNode>();
		foreach (BaseUnitEntity item in Game.Instance.State.AllBaseAwakeUnits.Where((BaseUnitEntity i) => i.IsInCombat && i.IsPlayerEnemy))
		{
			foreach (CustomGridNodeBase item2 in GridAreaHelper.GetNodesSpiralAround(item.Position.GetNearestNodeXZUnwalkable(), item.SizeRect, 1))
			{
				if (item.DistanceToInCells(item2.Vector3Position) <= 1)
				{
					hashSet.Add(item2);
				}
			}
		}
		return hashSet.ToArray();
	}

	private void HideMovableArea()
	{
		EventBus.RaiseEvent(delegate(IUnitMovableAreaHandler h)
		{
			h.HandleRemoveUnitMovableArea();
		});
	}

	public void HandleTurnBasedModeResumed()
	{
		TurnController turnController = Game.Instance.TurnController;
		if (turnController.IsPreparationTurn)
		{
			HandleBeginPreparationTurn(turnController.IsDeploymentAllowed);
			return;
		}
		Clear();
		MechanicEntity currentUnit = turnController.CurrentUnit;
		if (turnController.TurnBasedModeActive && currentUnit is BaseUnitEntity currentUnit2 && currentUnit.IsDirectlyControllable)
		{
			m_CurrentUnit = currentUnit2;
			UpdateMovableArea();
		}
	}

	public void HandleBeginPreparationTurn(bool canDeploy)
	{
		m_DeploymentPhase = canDeploy;
		if (canDeploy)
		{
			m_CurrentUnitSubscription?.Dispose();
			m_CurrentUnitSubscription = Game.Instance.SelectionCharacter.SelectedUnit.Subscribe(HandleNewUnitStartTurn);
		}
	}

	public void HandleEndPreparationTurn()
	{
		m_CurrentUnitSubscription?.Dispose();
		m_DeploymentPhase = false;
		ClearInitialPositions();
	}

	public void ClearInitialPositions()
	{
		m_InitialPositions.Clear();
	}

	public void Clear()
	{
		HideMovableArea();
		UnitPathManager.Instance.RemoveAllPaths();
		UnitCommandsRunner.CancelMoveCommand();
		m_CurrentUnit = null;
	}

	public bool TryGetInitialPosition(BaseUnitEntity unit, out Vector3 initialPosition)
	{
		initialPosition = Vector3.zero;
		if (m_InitialPositions == null || unit == null)
		{
			return false;
		}
		return m_InitialPositions.TryGetValue(unit.UniqueId, out initialPosition);
	}

	public void ApplyInitialPosition(BaseUnitEntity unit, Vector3 initialPosition)
	{
		if (m_InitialPositions != null && unit != null)
		{
			m_InitialPositions.TryAdd(unit.UniqueId, initialPosition);
			UpdateMovableAreaIfNeeded(unit);
		}
	}

	void INetRoleSetHandler.HandleRoleSet(string entityId)
	{
		UnitCommandsRunner.HandleRoleSet(entityId);
	}

	public void HandleUnitAbilityJumpDidActed(int distanceInCells)
	{
		if (ShouldHandle(EventInvokerExtensions.BaseUnitEntity))
		{
			UpdateMovableArea();
		}
	}

	public void HandleUnitResultJump(int distanceInCells, Vector3 targetPoint, bool directJump, MechanicEntity target, MechanicEntity caster, bool useAttack)
	{
	}

	public void HandleUnitEndTurn(bool isTurnBased)
	{
		Clear();
	}

	public void HandleUnitEndInterruptTurn()
	{
		Clear();
	}

	public void HandlePlayerInputLocked()
	{
		HideMovableArea();
	}

	public void HandlePlayerInputUnlocked()
	{
		if (m_CurrentUnit != null)
		{
			UpdateMovableArea();
		}
	}
}
