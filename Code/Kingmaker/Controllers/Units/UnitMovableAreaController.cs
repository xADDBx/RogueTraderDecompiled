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
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UniRx;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitMovableAreaController : IControllerDisable, IController, ITurnBasedModeHandler, ISubscriber, ITurnBasedModeResumeHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInterruptTurnStartHandler, IUnitCommandStartHandler, IUnitCommandEndHandler, IUnitCommandActHandler, IUnitActionPointsHandler, IUnitSpentMovementPoints, IUnitGainMovementPoints, IPreparationTurnBeginHandler, IPreparationTurnEndHandler, IDirectMovementHandler
{
	private BaseUnitEntity m_CurrentUnit;

	private bool m_DeploymentPhase;

	private IDisposable m_CurrentUnitSubscription;

	private Dictionary<string, Vector3> m_InitialPositions = new Dictionary<string, Vector3>();

	public List<GraphNode> CurrentUnitMovableArea { get; private set; }

	public List<GraphNode> DeploymentForbiddenArea { get; private set; }

	public void OnDisable()
	{
		Clear();
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (ShouldHandle(command.Executor) && (command is UnitMoveToProper || command is UnitUseAbility))
		{
			HideMovableArea();
		}
	}

	public void HandleUnitCommandDidAct(AbstractUnitCommand command)
	{
		if (!(command is UnitUseAbility))
		{
			UpdateMovableAreaIfNeeded(command.Executor);
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

	public void HandleUnitStartInterruptTurn()
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	private void HandleNewUnitStartTurn(MechanicEntity entity)
	{
		if (Game.Instance.TurnController.TurnBasedModeActive)
		{
			Clear();
			MechanicEntity mechanicEntity = (Game.Instance.TurnController.IsPreparationTurn ? (entity ?? Game.Instance.TurnController.CurrentUnit) : (Game.Instance.TurnController.CurrentUnit ?? entity));
			if (mechanicEntity is BaseUnitEntity currentUnit && mechanicEntity.IsDirectlyControllable)
			{
				m_CurrentUnit = currentUnit;
				UpdateMovableArea();
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			Clear();
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
		DeploymentForbiddenArea = (m_DeploymentPhase ? GetDeploymentForbiddenArea().ToList() : null);
		EventBus.RaiseEvent((IMechanicEntity)m_CurrentUnit, (Action<IUnitMovableAreaHandler>)delegate(IUnitMovableAreaHandler h)
		{
			h.HandleSetUnitMovableArea(CurrentUnitMovableArea);
		}, isCheckRuntime: true);
	}

	private List<GraphNode> GetMovableArea(BaseUnitEntity unit, Vector3 position)
	{
		int num = (int)unit.CombatState.ActionPointsBlue;
		if (num <= 0)
		{
			return null;
		}
		return PathfindingService.Instance.FindAllReachableTiles_Blocking(unit.View.MovementAgent, position, num).Keys.ToList();
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
		m_InitialPositions.Clear();
		m_DeploymentPhase = false;
	}

	public void Clear()
	{
		HideMovableArea();
		UnitPathManager.Instance.RemoveAllPaths();
		UnitCommandsRunner.CancelMoveCommand();
		m_CurrentUnit = null;
	}
}
