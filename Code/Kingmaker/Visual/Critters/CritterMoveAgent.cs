using System;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Visual.Critters;

public class CritterMoveAgent : MonoBehaviour
{
	[SerializeField]
	private float m_Acceleration = 1000f;

	[SerializeField]
	private float m_AngularSpeed = 720f;

	private PathfindingService.Options m_PathOptions;

	private RabbitLinkTraversalProvider m_NodeLinkTraverser;

	[CanBeNull]
	private ForcedPath m_Path;

	[CanBeNull]
	private Path m_RequestedPath;

	private bool m_RequestedNewPath;

	private float m_WarmupTime;

	private int m_NextPointIndex;

	private float m_Speed;

	private Vector2 m_NextWaypoint;

	private Vector2 m_NextNextWaypoint;

	private Vector2 m_NextDirection;

	private Vector3? m_Destination;

	public float MaxSpeed;

	private bool m_FirstTick;

	public Vector3 Position { get; private set; }

	public Vector2 Forward { get; private set; }

	public ForcedPath Path
	{
		get
		{
			return m_Path;
		}
		protected set
		{
			ReleasePath(m_Path);
			m_Path = value;
			ClaimPath(m_Path);
		}
	}

	public bool WantsToMove
	{
		get
		{
			if (Path == null)
			{
				return m_RequestedPath != null;
			}
			return true;
		}
	}

	public bool IsReallyMoving
	{
		get
		{
			if (Path != null && !Path.error)
			{
				return m_NextPointIndex > 0;
			}
			return false;
		}
	}

	private bool OnLastSegment
	{
		get
		{
			if (Path != null)
			{
				return m_NextPointIndex >= Path.vectorPath.Count - 1;
			}
			return false;
		}
	}

	public float Speed => m_Speed;

	private void ClaimPath([CanBeNull] Path path)
	{
		path?.Claim(this);
	}

	private void ReleasePath([CanBeNull] Path path)
	{
		path?.Release(this);
	}

	public void Init(Vector3 position, Vector2 forward)
	{
		Position = position;
		Forward = forward;
		if (m_PathOptions == null)
		{
			m_PathOptions = new PathfindingService.Options
			{
				Modifiers = new IPathModifier[2]
				{
					new StartEndModifier
					{
						exactStartPoint = StartEndModifier.Exactness.Original,
						exactEndPoint = StartEndModifier.Exactness.ClosestOnNode
					},
					new WarhammerFunnelModifier
					{
						shrink = 0.33f
					}
				}
			};
		}
		if (m_NodeLinkTraverser == null)
		{
			m_NodeLinkTraverser = new RabbitLinkTraversalProvider();
		}
	}

	public void PathTo(Vector3 start, Vector3 destination)
	{
		m_RequestedNewPath = m_Destination != destination;
		if (m_RequestedPath == null || m_RequestedNewPath)
		{
			m_Destination = destination;
			WarhammerABPath sourcePath = WarhammerABPath.Construct(start, destination);
			sourcePath.LinkTraversalProvider = m_NodeLinkTraverser;
			m_RequestedPath = sourcePath;
			PathfindingService.Instance.FindPath(m_RequestedPath, null, m_PathOptions, delegate(ForcedPath p)
			{
				OnPathComplete(p, sourcePath);
			});
		}
	}

	private void OnPathComplete([NotNull] ForcedPath p, Path sourcePath)
	{
		if (sourcePath != m_RequestedPath || !this)
		{
			return;
		}
		m_RequestedPath = null;
		m_RequestedNewPath = false;
		if (m_Destination.HasValue)
		{
			bool flag = !p.error;
			if (!p.error)
			{
				float num = GeometryUtils.Distance2D(p.vectorPath[p.vectorPath.Count - 1], m_Destination.Value);
				flag = flag && num <= 10f;
			}
			if (flag)
			{
				Path = p;
				SetWaypoint(1);
				m_FirstTick = true;
			}
		}
	}

	private void SetWaypoint(int index)
	{
		if (Path == null)
		{
			PFLog.Default.Warning("No path");
			return;
		}
		if (index <= 0 || index >= Path.vectorPath.Count)
		{
			PFLog.Default.Warning("Invalid path point {0}", index);
			return;
		}
		m_NextPointIndex = index;
		m_NextWaypoint = Path.vectorPath[index].To2D();
		if (index == Path.vectorPath.Count - 1)
		{
			m_NextNextWaypoint = m_NextWaypoint;
			m_NextDirection = Vector2.zero;
		}
		else
		{
			m_NextNextWaypoint = Path.vectorPath[index + 1].To2D();
			m_NextDirection = m_NextNextWaypoint - m_NextWaypoint;
			m_NextDirection.Normalize();
		}
	}

	public void TickMovement(float deltaTime)
	{
		if (m_WarmupTime > 0f)
		{
			m_WarmupTime -= deltaTime;
		}
		else
		{
			if (!IsReallyMoving)
			{
				return;
			}
			bool firstTick = m_FirstTick;
			m_FirstTick = false;
			Vector2 vector = Position.To2D();
			Vector2 forward = Forward;
			float num = Vector2.Distance(m_NextWaypoint, vector);
			float num2 = num / Mathf.Max(m_Speed, 0.01f);
			bool flag;
			if (Mathf.Abs(num2) > 0.01f)
			{
				Vector2 vector2 = (m_NextWaypoint - vector) / num;
				float num3 = (OnLastSegment ? 0f : Vector2.Angle(forward, m_NextDirection)) / m_AngularSpeed;
				Vector2 vector3 = ((num3 > num2) ? m_NextDirection : vector2);
				float num4 = MaxSpeed;
				if (OnLastSegment && m_Speed > m_Acceleration * deltaTime && num2 < m_Speed / m_Acceleration)
				{
					num4 = Mathf.MoveTowards(m_Speed, 0f, m_Acceleration * deltaTime);
				}
				m_Speed = num4;
				forward = ((!(num4 > 0f) || firstTick) ? vector3 : ((Vector2)Vector3.RotateTowards(forward, vector3, m_AngularSpeed * deltaTime * (MathF.PI / 180f), 1f)));
				Vector3 vector4 = forward.To3D() * m_Speed;
				Position = UnitMovementAgentBase.Move(Position, vector4 * deltaTime, 0.3f, out var _);
				Vector2 a = Position.To2D();
				Forward = forward;
				Vector2 vector5 = Path.vectorPath[m_NextPointIndex - 1].To2D();
				Vector2 lhs = m_NextWaypoint - vector5;
				Vector2 rhs = m_NextWaypoint - Position.To2D();
				flag = Vector2.Dot(lhs, rhs) < 0f;
				flag = flag || num3 > num2;
				if (OnLastSegment)
				{
					flag &= Vector2.Distance(a, m_NextWaypoint) < 0.2f;
				}
			}
			else
			{
				flag = true;
			}
			if (OnLastSegment && num < 1f)
			{
				Stop();
			}
			else if (flag)
			{
				if (OnLastSegment)
				{
					Stop();
				}
				else
				{
					SetWaypoint(m_NextPointIndex + 1);
				}
			}
		}
	}

	public void Stop()
	{
		Path = null;
		m_RequestedPath = null;
		m_Speed = 0f;
		m_Destination = null;
		m_NextPointIndex = 0;
	}
}
