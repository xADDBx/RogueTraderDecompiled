using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UnitLogic.Commands.Base;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Abilities;

namespace Kingmaker.Controllers.SpaceCombat;

public class StarshipPathController : IControllerTick, IController, IControllerStop, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber, IUnitCommandStartHandler, ITurnStartHandler, IContinueTurnHandler, IInterruptTurnStartHandler, IUnitDeathHandler, IInterruptTurnContinueHandler
{
	private StarshipEntity m_CurrentStarship;

	private bool m_InternalDeadEnd;

	public bool IsCurrentShipInDeadEnd { get; private set; }

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void OnStop()
	{
		HideCurrentShipPath();
	}

	public void Tick()
	{
		m_CurrentStarship = Game.Instance.TurnController.CurrentUnit as StarshipEntity;
		if (m_CurrentStarship != null && m_CurrentStarship.IsDirectlyControllable && m_CurrentStarship.Navigation.ShouldUpdateReachableTiles())
		{
			m_CurrentStarship.Navigation.UpdateReachableTiles(delegate(Path p)
			{
				SetPath(p);
			});
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command.Executor == m_CurrentStarship)
		{
			IsCurrentShipInDeadEnd = false;
			HideCurrentShipPath();
		}
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command.Executor == m_CurrentStarship)
		{
			ShowCurrentShipPath();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	public void HandleUnitContinueTurn(bool isTurnBased)
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		HandleNewUnitStartTurn(EventInvokerExtensions.MechanicEntity);
	}

	private void HandleNewUnitStartTurn(MechanicEntity unit)
	{
		m_CurrentStarship = unit as StarshipEntity;
		if (m_CurrentStarship != null)
		{
			if (m_CurrentStarship.IsDirectlyControllable)
			{
				ShowCurrentShipPath();
			}
			else
			{
				HideCurrentShipPath();
			}
		}
	}

	void IUnitDeathHandler.HandleUnitDeath(AbstractUnitEntity unitEntity)
	{
		if (!(Game.Instance.CurrentMode != GameModeType.SpaceCombat) && m_CurrentStarship.IsDirectlyControllable)
		{
			m_CurrentStarship.Navigation.UpdateReachableTiles(delegate(Path p)
			{
				SetPath(p);
			});
		}
	}

	public void ShowCustomShipPath(Dictionary<GraphNode, CustomPathNode> path, GameObject defaultMarker)
	{
		ShipPathManager.Instance.SetPathMarkers(m_CurrentStarship, path, defaultMarker);
	}

	public void ShowCurrentShipPath()
	{
		if (m_CurrentStarship == null || !m_CurrentStarship.IsDirectlyControllable || Game.Instance.SelectedAbilityHandler?.Ability != null)
		{
			return;
		}
		if (m_CurrentStarship.Navigation.ShouldUpdateReachableTiles())
		{
			m_CurrentStarship.Navigation.UpdateReachableTiles(delegate
			{
				ShowCurrentShipPath();
			});
		}
		else
		{
			SetPath(m_CurrentStarship.Navigation.ReachableTiles);
		}
	}

	private void SetPath(Path p)
	{
		ShipPathManager.Instance.SetPathMarkers(m_CurrentStarship, p);
		if (UpdateIsShipInDeadEnd(p as ShipPath))
		{
			m_CurrentStarship.Navigation.UpdateReachableTiles(SetPath, unitMaxMovePoints: false, isDeadEnd: true);
			IsCurrentShipInDeadEnd = true;
		}
	}

	private void HideCurrentShipPath()
	{
		ShipPathManager.Instance.ClearPathMarkers();
	}

	private bool UpdateIsShipInDeadEnd(ShipPath path)
	{
		bool flag = (path?.Result?.Count((KeyValuePair<GraphNode, ShipPath.PathCell> x) => x.Value.CanStand)).GetValueOrDefault() > 1;
		bool flag2 = m_CurrentStarship.CombatState.ActionPointsBlue > (float)Game.Instance.BlueprintRoot.DeadEndTurnCost;
		m_InternalDeadEnd = !flag && flag2;
		return m_InternalDeadEnd;
	}
}
