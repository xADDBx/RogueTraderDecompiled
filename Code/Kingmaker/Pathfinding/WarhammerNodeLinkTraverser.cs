using System;
using Kingmaker.Blueprints;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.Animation;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public class WarhammerNodeLinkTraverser : ILinkTraversalProvider
{
	public enum State
	{
		None,
		WaitForTraverse,
		TraverseDownHorizontalIn,
		TraverseIn,
		Traverse,
		TraverseOut,
		TraverseUpHorizontalOut,
		Traversed
	}

	private static readonly float MinSpeed = 30.Feet().Meters / 2.5f;

	private const int MaxTraverseProgress = 1;

	private readonly UnitMovementAgentBase m_MovementAgent;

	private INodeLink m_LastTraversedPathLink;

	private GraphNode m_GraphNodeFrom;

	private GraphNode m_GraphNodeTo;

	private float m_TraverseTimer;

	private static bool IsCorrectMode => Game.Instance.CurrentMode == GameModeType.Default;

	public State LastState { get; private set; }

	public float OutUpVerticalClipDuration { get; set; }

	public float OutUpHorizontalClipDuration { get; set; }

	public float InUpVerticalClipDuration { get; set; }

	public float OutUpVerticalDistance { get; set; }

	public float InDownVerticalClipDuration { get; set; }

	public float InDownHorizontalClipDuration { get; set; }

	public float OutDownVerticalClipDuration { get; set; }

	public float InDownVerticalDistance { get; set; }

	public float VerticalSpeed { get; set; }

	public bool IsInQueue => LastState == State.WaitForTraverse;

	public bool IsTraverseNow
	{
		get
		{
			if (m_LastTraversedPathLink != null)
			{
				return m_LastTraversedPathLink.IsInTraverse(this);
			}
			return false;
		}
	}

	public MechanicEntity Traverser => Entity;

	public bool AllowOtherToUseLink
	{
		get
		{
			if (IsUpTraverse)
			{
				if ((LastState != State.Traverse || !(StateProgress > 0.5f)) && LastState != State.TraverseOut)
				{
					return LastState == State.TraverseUpHorizontalOut;
				}
				return true;
			}
			if ((LastState != State.TraverseDownHorizontalIn || !(StateProgress > 0.5f)) && LastState != State.Traverse)
			{
				return LastState == State.TraverseOut;
			}
			return true;
		}
	}

	private AbstractUnitEntity Entity
	{
		get
		{
			if (!(View != null))
			{
				return null;
			}
			return View.Data;
		}
	}

	private AbstractUnitEntityView View => m_MovementAgent.Unit;

	private float StateProgress
	{
		get
		{
			if (IsTraverseNow && GetStateDuration() > 0f)
			{
				return m_TraverseTimer / GetStateDuration();
			}
			return 1f;
		}
	}

	public bool IsUpTraverse => m_GraphNodeFrom.Vector3Position.y < m_GraphNodeTo.Vector3Position.y;

	public float TraverseHeight => Math.Abs(m_GraphNodeFrom.Vector3Position.y - m_GraphNodeTo.Vector3Position.y);

	public bool ForceNextState { get; set; }

	public IntRect SizeRect => Entity.SizeRect;

	public WarhammerNodeLinkTraverser(UnitMovementAgentBase movementAgent)
	{
		m_MovementAgent = movementAgent;
	}

	public void Reset()
	{
		if (IsTraverseNow)
		{
			CompleteTraverse();
			m_LastTraversedPathLink = null;
			LastState = State.Traversed;
		}
		else
		{
			ForceNextState = false;
			m_LastTraversedPathLink = null;
			LastState = State.None;
		}
	}

	public bool CanBuildPathThroughLink(GraphNode from, GraphNode to, INodeLink link)
	{
		if (!IsCorrectMode)
		{
			return false;
		}
		if (!link.ConnectsNodes(from, to))
		{
			return false;
		}
		if (!link.IsCorrectSize(this))
		{
			return false;
		}
		BlueprintArmyDescription army = Entity.Blueprint.Army;
		if ((army == null || !army.IsHumanoid) && !Entity.IsPlayerFaction)
		{
			return false;
		}
		return true;
	}

	public void Tick(float deltaTime)
	{
		if (IsCorrectMode)
		{
			switch (LastState)
			{
			case State.None:
				TryFindNode();
				break;
			case State.WaitForTraverse:
				TryStartTraverse();
				break;
			case State.TraverseIn:
				TraverseIn(deltaTime);
				break;
			case State.Traverse:
				Traverse(deltaTime);
				break;
			case State.TraverseOut:
				TraverseOut(deltaTime);
				break;
			case State.TraverseUpHorizontalOut:
				TraverseUpHorizontalOut(deltaTime);
				break;
			case State.TraverseDownHorizontalIn:
				TraverseDownHorizontalIn(deltaTime);
				break;
			case State.Traversed:
				LastState = State.None;
				break;
			}
		}
	}

	private void TryFindNode()
	{
		if (m_MovementAgent.IsReallyMoving && NodeLinksExtensions.AreConnected(m_MovementAgent.Position, m_MovementAgent.NextWaypoint, out var fromNode, out var toNode, out var currentLink) && m_LastTraversedPathLink != currentLink)
		{
			m_TraverseTimer = 0f;
			m_GraphNodeFrom = fromNode;
			m_GraphNodeTo = toNode;
			m_LastTraversedPathLink = currentLink;
			LastState = State.WaitForTraverse;
		}
	}

	private void TryStartTraverse()
	{
		if (!m_LastTraversedPathLink.IsCorrectSize(this))
		{
			PFLog.Default.Warning("Unit has incorrect path. Traverse link width is less than needed for this unit to pass.");
			m_MovementAgent.Stop();
			LastState = State.None;
		}
		else if (m_LastTraversedPathLink.CanStartTraverse(this))
		{
			if (View.AnimationManager != null)
			{
				View.AnimationManager.IsTraverseLink = true;
			}
			m_LastTraversedPathLink.StartTransition(this);
			LastState = (IsUpTraverse ? State.TraverseIn : State.TraverseDownHorizontalIn);
		}
	}

	private void TraverseDownHorizontalIn(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || ForceNextState)
		{
			ForceNextState = false;
			m_TraverseTimer = 0f;
			LastState = State.TraverseIn;
		}
	}

	private void TraverseIn(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || ForceNextState)
		{
			ForceNextState = false;
			m_TraverseTimer = 0f;
			LastState = State.Traverse;
		}
	}

	private void Traverse(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || ForceNextState)
		{
			ForceNextState = false;
			m_TraverseTimer = 0f;
			LastState = State.TraverseOut;
		}
	}

	private void TraverseOut(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || ForceNextState)
		{
			ForceNextState = false;
			m_TraverseTimer = 0f;
			LastState = State.TraverseUpHorizontalOut;
		}
	}

	private void TraverseUpHorizontalOut(float deltaTime)
	{
		ProcessTraverse(deltaTime);
		if (!(StateProgress < 1f) || ForceNextState)
		{
			CompleteTraverse();
		}
	}

	private void ProcessTraverse(float deltaTime)
	{
		m_TraverseTimer += deltaTime;
		Vector3 mechanicsPosition = CalculateTraverseViewPosition2D().To3D(GetTraverseY());
		Vector3 mechanicsPosition2 = Entity.Position + m_GraphNodeTo.Vector3Position - m_GraphNodeFrom.Vector3Position;
		m_MovementAgent.Position = SizePathfindingHelper.FromViewToMechanicsPosition(Entity, mechanicsPosition);
		Entity.ForceLookAt(SizePathfindingHelper.FromViewToMechanicsPosition(Entity, mechanicsPosition2));
		if (m_MovementAgent.Unit != null)
		{
			IKController ikController = m_MovementAgent.Unit.IkController;
			if ((object)ikController != null && (object)ikController.GrounderIk != null)
			{
				ikController.enabled = false;
				ikController.GrounderIk.enabled = false;
			}
		}
	}

	public float GetTraverseSpeedMps()
	{
		if (VerticalSpeed != 0f)
		{
			return VerticalSpeed;
		}
		if (MinSpeed > Entity.MovementAgent.MaxSpeed)
		{
			return Math.Max(MinSpeed, Entity.Movable.ModifiedSpeedMps);
		}
		return Math.Clamp(Entity.Movable.ModifiedSpeedMps, MinSpeed, Entity.MovementAgent.MaxSpeed);
	}

	public float GetStateDuration()
	{
		switch (LastState)
		{
		case State.TraverseIn:
			if (IsUpTraverse)
			{
				return InUpVerticalClipDuration;
			}
			return InDownVerticalClipDuration;
		case State.TraverseDownHorizontalIn:
			if (IsUpTraverse)
			{
				return 0f;
			}
			return InDownHorizontalClipDuration;
		case State.Traverse:
			if (IsUpTraverse)
			{
				return (m_LastTraversedPathLink.GetHeight() - InDownVerticalDistance) / GetTraverseSpeedMps();
			}
			return (m_LastTraversedPathLink.GetHeight() - OutUpVerticalDistance) / GetTraverseSpeedMps();
		case State.TraverseOut:
			if (!IsUpTraverse)
			{
				return OutDownVerticalClipDuration;
			}
			return OutUpVerticalClipDuration;
		case State.TraverseUpHorizontalOut:
			if (!IsUpTraverse)
			{
				return 0f;
			}
			return OutUpHorizontalClipDuration;
		default:
			throw new NotImplementedException("Only traverse states have duration");
		}
	}

	private float GetTraverseY()
	{
		switch (LastState)
		{
		case State.TraverseDownHorizontalIn:
			return m_GraphNodeFrom.Vector3Position.y;
		case State.TraverseIn:
			if (IsUpTraverse)
			{
				return m_GraphNodeFrom.Vector3Position.y;
			}
			return Mathf.Lerp(m_GraphNodeFrom.Vector3Position.y, m_GraphNodeFrom.Vector3Position.y - InDownVerticalDistance, StateProgress);
		case State.Traverse:
			if (!IsUpTraverse)
			{
				return Mathf.Lerp(m_GraphNodeFrom.Vector3Position.y - InDownVerticalDistance, m_GraphNodeTo.Vector3Position.y, StateProgress);
			}
			return Mathf.Lerp(m_GraphNodeFrom.Vector3Position.y, m_GraphNodeTo.Vector3Position.y - OutUpVerticalDistance, StateProgress);
		case State.TraverseOut:
			if (!IsUpTraverse)
			{
				return m_GraphNodeTo.Vector3Position.y;
			}
			return Mathf.Lerp(m_GraphNodeTo.Vector3Position.y - OutUpVerticalDistance, m_GraphNodeTo.Vector3Position.y, StateProgress);
		case State.TraverseUpHorizontalOut:
			return m_GraphNodeTo.Vector3Position.y;
		default:
			throw new NotImplementedException("Only traverse states has traverse position");
		}
	}

	private Vector2 CalculateTraverseViewPosition2D()
	{
		Vector2 vector = m_LastTraversedPathLink.GetConnectionPosition(this).To2D();
		Vector2 vector2 = m_LastTraversedPathLink.EndToStartDirection.To2D() * (m_LastTraversedPathLink.Bounds.extents.z + 0.35f);
		if (IsUpTraverse)
		{
			if (LastState == State.TraverseUpHorizontalOut)
			{
				return Vector2.Lerp(vector + vector2, m_GraphNodeTo.Vector3Position.To2D(), StateProgress);
			}
		}
		else if (LastState == State.TraverseDownHorizontalIn)
		{
			return Vector2.Lerp(m_GraphNodeFrom.Vector3Position.To2D(), vector + vector2, StateProgress);
		}
		return vector + vector2;
	}

	private void CompleteTraverse()
	{
		if (m_MovementAgent.Unit != null)
		{
			IKController ikController = m_MovementAgent.Unit.IkController;
			if ((object)ikController != null && (object)ikController.GrounderIk != null)
			{
				ikController.enabled = true;
				ikController.GrounderIk.enabled = true;
			}
		}
		if (View.AnimationManager != null)
		{
			View.AnimationManager.IsTraverseLink = false;
		}
		ForceNextState = false;
		m_TraverseTimer = 0f;
		m_MovementAgent.Position = m_GraphNodeTo.Vector3Position;
		m_LastTraversedPathLink.CompleteTransition(this);
		LastState = State.Traversed;
	}

	private bool IsApproachFree(GraphNode to)
	{
		if (SizeRect.Width == 1)
		{
			return !WarhammerBlockManager.Instance.NodeContainsAnyExcept(to, m_MovementAgent.Blocker, enemies: true);
		}
		foreach (CustomGridNodeBase node in GridAreaHelper.GetNodes(to.Vector3Position, SizeRect))
		{
			if (node != null && WarhammerBlockManager.Instance.NodeContainsAnyExcept(node, m_MovementAgent.Blocker, enemies: true))
			{
				return false;
			}
		}
		return true;
	}
}
