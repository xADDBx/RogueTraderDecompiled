using System;
using JetBrains.Annotations;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

[DisallowMultipleComponent]
public class UnitMovementAgentContinuous : UnitMovementAgentBase
{
	public const float Epsilon = 0.0001f;

	public const float AccelerationThreshold = 0.7f;

	private bool m_EnableSlidingAssist;

	private int m_SlidingAssistDirection;

	private float m_CurrentSlidingAngle;

	private const float SlidingAngleLimit = 90f;

	private CustomGridNodeBase m_CurrentNode;

	public override bool WantsToMove => DirectionFromControllerMagnitude > 0.0001f;

	public override bool IsReallyMoving
	{
		get
		{
			if (!WantsToMove && !(m_Velocity.sqrMagnitude > 0.0001f))
			{
				return base.IsTraverseInProgress;
			}
			return true;
		}
	}

	public Vector2 DirectionFromController { get; set; }

	public float DirectionFromControllerMagnitude { get; set; }

	[UsedImplicitly]
	private void OnEnable()
	{
		UnitMovementAgentBase.AllAgents.Add(this);
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		UnitMovementAgentBase.AllAgents.Remove(this);
		ObstaclesHelper.RemoveFromGroup(this);
	}

	public override void TickMovement(float deltaTime)
	{
		if (Game.Instance.CurrentMode != GameModeType.Default)
		{
			return;
		}
		m_NodeLinkTraverser.Tick(deltaTime);
		if (!(base.Unit?.AnimationManager?.IsPreventingMovement).GetValueOrDefault())
		{
			AbstractUnitEntityView unit = base.Unit;
			if ((object)unit == null || !unit.IsCommandsPreventMovement)
			{
				using (ProfileScope.New("Tick Movement Continous"))
				{
					AbstractUnitEntity abstractUnitEntity = base.Unit?.Data;
					if (abstractUnitEntity != null)
					{
						Position = abstractUnitEntity.Position;
					}
					if (!IsReallyMoving)
					{
						Stop();
					}
					else
					{
						bool firstTick = m_FirstTick;
						m_FirstTick = false;
						float num = base.MaxSpeedOverride ?? GetSpeedByControllerStickDeflection(DirectionFromControllerMagnitude);
						if (firstTick)
						{
							m_Speed = num;
							Vector3 clampedPosition = AstarPath.active.graphs[0].GetNearest(Position).clampedPosition;
							Position = clampedPosition;
						}
						Vector2 directionFromController = DirectionFromController;
						Vector2 vector = directionFromController;
						Vector2 vector2 = vector;
						base.IsStopping = false;
						float speed = ((Mathf.Abs(num - m_Speed) < m_Acceleration * deltaTime) ? num : (m_Speed + Mathf.Sign(num - m_Speed) * m_Acceleration * deltaTime));
						if (vector.SqrMagnitude() < Mathf.Epsilon)
						{
							base.IsStopping = true;
						}
						if (abstractUnitEntity != null && m_EnableSlidingAssist)
						{
							UpdateSliding(Position, vector2, deltaTime);
							vector2 = vector2.RotateAroundPoint(Vector2.zero, m_CurrentSlidingAngle);
						}
						vector2 = Vector2.Lerp(directionFromController, vector2, 0.5f);
						float num2 = (base.CombatMode ? m_CombatAngularSpeed : m_AngularSpeed);
						directionFromController = Vector3.RotateTowards(directionFromController, vector2, num2 * deltaTime * (MathF.PI / 180f), 1f);
						m_Speed = speed;
						m_NextVelocity = directionFromController.To3D() * m_Speed;
						CustomGridNodeBase targetNode;
						Vector3 vector3 = UnitMovementAgentBase.Move(Position, m_NextVelocity * deltaTime, base.Corpulence, out targetNode);
						Vector3 vector4 = vector3 - (Position + m_NextVelocity * deltaTime);
						IsPositionChanged = Mathf.Abs(vector4.x) < 0.01f && Mathf.Abs(vector4.z) < 0.01f;
						if (IsPositionChanged)
						{
							m_EnableSlidingAssist = false;
							m_CurrentSlidingAngle = 0f;
							m_SlidingAssistDirection = 0;
						}
						else
						{
							m_EnableSlidingAssist = true;
						}
						if (!NodeLinksExtensions.AreConnected(m_CurrentNode, targetNode, out var _))
						{
							Position = vector3;
							m_CurrentNode = targetNode;
						}
						base.MoveDirection = directionFromController;
					}
					return;
				}
			}
		}
		Stop();
	}

	private float GetSpeedByControllerStickDeflection(float stickDeflection)
	{
		if (stickDeflection > 0.7f)
		{
			float a = base.MaxSpeed - 15f * (1f - DirectionFromControllerMagnitude);
			return Mathf.Max(a, 5f);
		}
		return base.MaxSpeed * (1f + 2f * DirectionFromControllerMagnitude);
	}

	private void UpdateSliding(Vector3 position, Vector2 desiredDir, float deltaTime)
	{
		if (m_SlidingAssistDirection == 0)
		{
			GraphNode node = ObstacleAnalyzer.GetNearestNode(position).node;
			Vector3 end = position + desiredDir.RotateAroundPoint(Vector2.zero, -90f).To3D().normalized;
			Vector3 end2 = position + desiredDir.RotateAroundPoint(Vector2.zero, 90f).To3D().normalized;
			Linecast.LinecastGrid(node.Graph, position, end, node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
			Linecast.LinecastGrid(node.Graph, position, end2, node, out var hit2, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
			m_SlidingAssistDirection = ((!(hit.distance > hit2.distance)) ? 1 : (-1));
		}
		m_CurrentSlidingAngle = Mathf.Clamp(m_CurrentSlidingAngle + deltaTime * (float)m_SlidingAssistDirection * 45f, -90f, 90f);
	}
}
