using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.CodeTimer;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public struct ObstacleAnalyzer
{
	public struct Input
	{
		public Vector3 Position3D;

		public Vector2 Position2D;

		public Vector2 Forward;

		public float Speed;

		public float Corpulence;

		public Input(Vector3 position, Vector2 forward, float speed, float corpulence)
		{
			Position3D = position;
			Position2D = position.To2D();
			Forward = forward;
			Speed = speed;
			Corpulence = corpulence;
		}
	}

	public struct Output
	{
		internal bool MainDirectionBlocked;

		public bool MainDirectionBlockedByCore;

		public bool MainDirectionBlockedByStatic;

		public bool ShouldSlowDown;

		internal List<ObstaclePoint> LeftPoints;

		internal List<ObstaclePoint> RightPoints;
	}

	public ref struct Obstacle
	{
		public bool IsMoving;

		public bool SlowsDown;
	}

	private readonly struct IsCellBlocked : Linecast.ICanTransitionBetweenCells
	{
		private readonly WarhammerSingleNodeBlocker m_CallerNodeBlocker;

		public IsCellBlocked(WarhammerSingleNodeBlocker callerNodeBlocker)
		{
			m_CallerNodeBlocker = callerNodeBlocker;
		}

		public bool CanTransitionBetweenCells(CustomGridNodeBase nodeFrom, CustomGridNodeBase nodeTo, Vector3 transitionPosition, float distanceFactor)
		{
			if ((bool)WarhammerBlockManager.Instance)
			{
				return WarhammerBlockManager.Instance.NodeContainsAnyExcept(nodeTo, m_CallerNodeBlocker);
			}
			return true;
		}
	}

	private static readonly LogChannel Logger;

	public const float SoftCoreCorpulenceDelta = 10f;

	public const float DefaultCoreCorpulenceDelta = 0.3f;

	internal const float MaxAngle = 175f;

	private const float KeepDirectionAngle = 180f;

	private const float CollisionTime = 1.5f;

	[NotNull]
	internal static readonly NNConstraint DefaultXZConstraint;

	[NotNull]
	internal static readonly NNConstraint UnwalkableXZConstraint;

	private Input m_Input;

	private Output m_Output;

	public bool MainDirectionBlockedByStatic => m_Output.MainDirectionBlockedByStatic;

	public bool MainDirectionBlockedByCore => m_Output.MainDirectionBlockedByCore;

	public bool ShouldSlowDown => m_Output.ShouldSlowDown;

	public static float CellSize => ((CustomGridGraph)AstarPath.active.graphs[0]).nodeSize;

	static ObstacleAnalyzer()
	{
		Logger = LogChannelFactory.GetOrCreate("ObstacleAnalyzer");
		DefaultXZConstraint = NNConstraint.Default;
		UnwalkableXZConstraint = NNConstraint.Default;
		UnwalkableXZConstraint.distanceXZ = (DefaultXZConstraint.distanceXZ = true);
		UnwalkableXZConstraint.constrainTags = (DefaultXZConstraint.constrainTags = true);
		UnwalkableXZConstraint.tags = (DefaultXZConstraint.tags = 1);
		UnwalkableXZConstraint.constrainWalkability = false;
	}

	public ObstacleAnalyzer(Vector3 position, Vector2 forward, float radius, float speed)
	{
		m_Input = new Input(position, forward, speed, radius);
		m_Output = new Output
		{
			LeftPoints = TempList.Get<ObstaclePoint>(),
			RightPoints = TempList.Get<ObstaclePoint>()
		};
	}

	public void AddObstacle(Vector2 pB, Vector2 velB, float corpulenceSum, float coreCorpulenceDelta = 0.3f)
	{
		float coreCorpulenceMultiplier = Mathf.Max(0.01f, (corpulenceSum - coreCorpulenceDelta) / corpulenceSum);
		Vector2 vector = m_Input.Position2D - pB;
		if (velB.sqrMagnitude < 0.0001f || corpulenceSum > vector.magnitude)
		{
			Obstacle obstacle = default(Obstacle);
			AddStaticObstacle(pB, corpulenceSum, coreCorpulenceMultiplier, ref obstacle);
		}
		else if (Mathf.Abs(velB.sqrMagnitude / (m_Input.Speed * m_Input.Speed) - 1f) < 0.1f)
		{
			bool slowsDown = GeometryUtils.Cross2D(m_Input.Forward, velB) > 0f;
			Obstacle obstacle2 = default(Obstacle);
			obstacle2.IsMoving = true;
			obstacle2.SlowsDown = slowsDown;
			Obstacle obstacle3 = obstacle2;
			AddObstacleSameSpeed(pB, velB, corpulenceSum, coreCorpulenceMultiplier, ref obstacle3);
		}
		else
		{
			Obstacle obstacle2 = default(Obstacle);
			obstacle2.IsMoving = true;
			obstacle2.SlowsDown = false;
			Obstacle obstacle4 = obstacle2;
			AddObstacleDifferentSpeeds(pB, velB, corpulenceSum, coreCorpulenceMultiplier, ref obstacle4);
		}
	}

	private void AddStaticObstacle(Vector2 pB, float corpulenceSum, float coreCorpulenceMultiplier, ref Obstacle obstacle)
	{
		Vector2 b = pB - m_Input.Position2D;
		float magnitude = b.magnitude;
		float num = GeometryUtils.SignedAngle(m_Input.Forward, b);
		float f = Mathf.Min(1f, corpulenceSum / magnitude);
		float num2 = 57.29578f * Mathf.Asin(f);
		float f2 = Mathf.Min(1f, corpulenceSum * coreCorpulenceMultiplier / magnitude);
		float num3 = 57.29578f * Mathf.Asin(f2);
		MarkObstacle(num - num2, num + num2, ObstacleMode.Outer, ref obstacle);
		MarkObstacle(num - num3, num + num3, ObstacleMode.Core, ref obstacle);
	}

	private void AddObstacleSameSpeed(Vector2 pB, Vector2 velB, float corpulenceSum, float coreCorpulenceMultiplier, ref Obstacle obstacle)
	{
		Vector2 lhs = m_Input.Position2D - pB;
		float num = Vector2.Dot(lhs, velB);
		if (!(num < 0.0001f))
		{
			float num2 = lhs.sqrMagnitude / (2f * num);
			if (!(num2 > 1.5f))
			{
				Vector2 pB2 = pB + num2 * velB;
				float corpulenceSum2 = 2f * corpulenceSum * (num2 * velB).magnitude / lhs.magnitude;
				AddStaticObstacle(pB2, corpulenceSum2, coreCorpulenceMultiplier, ref obstacle);
			}
		}
	}

	private void AddObstacleDifferentSpeeds(Vector2 pB, Vector2 velB, float corpulenceSum, float coreCorpulenceMultiplier, ref Obstacle obstacle)
	{
		Vector2 lhs = pB - m_Input.Position2D;
		float num = Vector2.Dot(lhs, velB);
		float num2 = velB.sqrMagnitude - m_Input.Speed * m_Input.Speed;
		float num3 = 2f * num;
		float sqrMagnitude = lhs.sqrMagnitude;
		float num4 = num3 * num3 - 4f * num2 * sqrMagnitude;
		if (!(num4 < 0f) && !(Mathf.Abs(num2) < 0.0001f))
		{
			float num5 = (0f - num3 + (float)Math.Sqrt(num4)) / (2f * num2);
			float num6 = (0f - num3 - (float)Math.Sqrt(num4)) / (2f * num2);
			if (num5 > 0f && num5 < 1.5f)
			{
				AddStaticObstacle(pB + velB * num5, corpulenceSum, coreCorpulenceMultiplier, ref obstacle);
			}
			if (num6 > 0f && num6 < 1.5f)
			{
				AddStaticObstacle(pB + velB * num6, corpulenceSum, coreCorpulenceMultiplier, ref obstacle);
			}
		}
	}

	public float CalcAvoidanceDirection(float lastAngle)
	{
		if (!m_Output.MainDirectionBlocked)
		{
			return 0f;
		}
		m_Output.LeftPoints.Sort(ObstaclePoint.Comparer);
		m_Output.RightPoints.Sort(ObstaclePoint.Comparer);
		if (!m_Output.MainDirectionBlockedByCore)
		{
			SideResult sideResult = CalcForwardSqeeze(m_Output.LeftPoints);
			SideResult sideResult2 = CalcForwardSqeeze(m_Output.RightPoints);
			if (sideResult.Valid && sideResult2.Valid)
			{
				return sideResult.Angle + sideResult2.Angle;
			}
		}
		SideResult sideResult3 = CalcSideDirection(m_Output.LeftPoints);
		SideResult sideResult4 = CalcSideDirection(m_Output.RightPoints);
		if (lastAngle < 0f && sideResult3.Valid && Mathf.Abs(lastAngle - sideResult3.Angle) < 180f)
		{
			return sideResult3.Angle;
		}
		if (lastAngle > 0f && sideResult4.Valid && Mathf.Abs(lastAngle - sideResult4.Angle) < 180f)
		{
			return sideResult4.Angle;
		}
		if (!(Math.Abs(sideResult3.Angle) < Math.Abs(sideResult4.Angle)))
		{
			return sideResult4.Angle;
		}
		return sideResult3.Angle;
	}

	public bool AddNavmeshObstacles()
	{
		if (!AstarPath.active || !Game.Instance.CurrentlyLoadedArea.IsNavmeshArea)
		{
			return false;
		}
		NNInfo nearestNode = GetNearestNode(m_Input.Position3D);
		if (nearestNode.node == null)
		{
			LogChannel @default = PFLog.Default;
			Vector2 position2D = m_Input.Position2D;
			@default.Warning("Could not find navmesh position for: " + position2D.ToString());
			return false;
		}
		Vector2 b = m_Input.Position2D - nearestNode.position.To2D();
		if (b.sqrMagnitude > 0.0001f)
		{
			float num = GeometryUtils.SignedAngle(m_Input.Forward, b);
			Obstacle obstacle = default(Obstacle);
			obstacle.IsMoving = false;
			obstacle.SlowsDown = false;
			Obstacle obstacle2 = obstacle;
			MarkObstacle(num - 100f, num + 100f, ObstacleMode.Core, ref obstacle2);
			return true;
		}
		return false;
	}

	private void MarkObstacle(float startAngle, float endAngle, ObstacleMode mode, ref Obstacle obstacle)
	{
		startAngle = NormalizeAngle(startAngle);
		endAngle = NormalizeAngle(endAngle);
		if (startAngle > endAngle)
		{
			if (endAngle > -179f)
			{
				MarkObstacle(-179f, endAngle, mode, ref obstacle);
			}
			if (startAngle < 179f)
			{
				MarkObstacle(startAngle, 179f, mode, ref obstacle);
			}
		}
		else if (startAngle < 0f && endAngle > 0f)
		{
			m_Output.MainDirectionBlocked = true;
			if (mode == ObstacleMode.Core)
			{
				m_Output.MainDirectionBlockedByCore = true;
			}
			if (!obstacle.IsMoving)
			{
				m_Output.MainDirectionBlockedByStatic = true;
			}
			else if (obstacle.SlowsDown)
			{
				m_Output.ShouldSlowDown = true;
			}
			m_Output.LeftPoints.Add(new ObstaclePoint(0f, ObstaclePointType.Start, mode));
			m_Output.LeftPoints.Add(new ObstaclePoint(startAngle, ObstaclePointType.End, mode));
			m_Output.RightPoints.Add(new ObstaclePoint(0f, ObstaclePointType.Start, mode));
			m_Output.RightPoints.Add(new ObstaclePoint(endAngle, ObstaclePointType.End, mode));
		}
		else if (startAngle < 0f)
		{
			m_Output.LeftPoints.Add(new ObstaclePoint(endAngle, ObstaclePointType.Start, mode));
			m_Output.LeftPoints.Add(new ObstaclePoint(startAngle, ObstaclePointType.End, mode));
		}
		else if (endAngle > 0f)
		{
			m_Output.RightPoints.Add(new ObstaclePoint(startAngle, ObstaclePointType.Start, mode));
			m_Output.RightPoints.Add(new ObstaclePoint(endAngle, ObstaclePointType.End, mode));
		}
	}

	private SideResult CalcSideDirection(List<ObstaclePoint> side)
	{
		if (side.Count <= 0)
		{
			return new SideResult(0f, sqeeze: false);
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		float num4 = 0f;
		for (int i = 0; i < side.Count; i++)
		{
			int num5 = num2;
			ObstaclePoint obstaclePoint = side[i];
			if (obstaclePoint.Mode == ObstacleMode.Core)
			{
				num2 = (int)(num2 + obstaclePoint.Type);
				if (num2 == 0)
				{
					num4 = obstaclePoint.Angle;
				}
			}
			if (obstaclePoint.Mode == ObstacleMode.Outer)
			{
				num3 = (int)(num3 + obstaclePoint.Type);
			}
			if (num3 == 0 && num2 == 0)
			{
				return new SideResult(obstaclePoint.Angle, sqeeze: false);
			}
			if (num2 == 1 && num5 == 0)
			{
				num++;
				if (num > 1 && Mathf.Abs(obstaclePoint.Angle) > 0f)
				{
					return new SideResult((obstaclePoint.Angle + num4) / 2f, sqeeze: true);
				}
			}
		}
		return SideResult.Invalid();
	}

	private SideResult CalcForwardSqeeze(List<ObstaclePoint> side)
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < side.Count; i++)
		{
			ObstaclePoint obstaclePoint = side[i];
			if (obstaclePoint.Mode == ObstacleMode.Core)
			{
				num = (int)(num + obstaclePoint.Type);
			}
			if (obstaclePoint.Mode == ObstacleMode.Outer)
			{
				num2 = (int)(num2 + obstaclePoint.Type);
			}
			if (num2 == 0 && num == 0)
			{
				return SideResult.Invalid();
			}
			if (num > 0)
			{
				return new SideResult(obstaclePoint.Angle / 2f, sqeeze: true);
			}
		}
		return SideResult.Invalid();
	}

	public void DrawDebug(Vector3 p, Vector2 forward)
	{
		DrawDebugPoints(p, forward);
	}

	public static Vector3 TraceAlongNavmesh(Vector3 start, Vector3 end, GraphNode hint = null, WarhammerSingleNodeBlocker callerNodeBlocker = null)
	{
		NNInfo nearestNode = GetNearestNode(start, hint);
		if ((start.To2D() - nearestNode.position.To2D()).sqrMagnitude > 0.0001f)
		{
			return nearestNode.position;
		}
		GraphHitInfo hit;
		if (nearestNode.node.Graph is CustomGridGraph)
		{
			if (callerNodeBlocker != null)
			{
				IsCellBlocked condition = new IsCellBlocked(callerNodeBlocker);
				Linecast.LinecastGrid(nearestNode.node.Graph, start, end, nearestNode.node, out hit, DefaultXZConstraint, ref condition);
			}
			else
			{
				Linecast.LinecastGrid(nearestNode.node.Graph, start, end, nearestNode.node, out hit, DefaultXZConstraint);
			}
		}
		else
		{
			NavmeshBase.Linecast(nearestNode.node.Graph, start, end, nearestNode.node, out hit, null);
		}
		return hit.point;
	}

	public static Int2 GetNearestNodeCoords(Vector3 position)
	{
		AstarPath active = AstarPath.active;
		if (!active)
		{
			Logger.Error("No active AStar");
			return default(Int2);
		}
		NavGraph[] graphs = active.graphs;
		if (graphs == null || graphs.Length > 1 || graphs[0] == null)
		{
			Logger.Error("No valid Grid Graph");
			return default(Int2);
		}
		if (graphs[0] is CustomGridGraph customGridGraph)
		{
			return customGridGraph.GetNearestNodeCoords(position);
		}
		Logger.Error("No valid Grid Graph");
		return default(Int2);
	}

	[CanBeNull]
	public static CustomGridNode GetNearestNodeXZUnwalkable(Vector3 pos)
	{
		AstarPath active = AstarPath.active;
		if (!active)
		{
			return null;
		}
		NavGraph[] graphs = active.graphs;
		if (graphs == null || graphs.Length < 1 || graphs[0] == null)
		{
			return null;
		}
		if (graphs[0] is CustomGridGraph customGridGraph)
		{
			return customGridGraph.GetNearestDirect(pos);
		}
		return null;
	}

	public static NNInfo GetNearestNode(Vector3 pos, GraphNode hint = null, NNConstraint constraint = null)
	{
		if (constraint == null)
		{
			constraint = DefaultXZConstraint;
		}
		using (ProfileScope.NewScope("GetNearestNode"))
		{
			if (hint is CustomGridNodeBase customGridNodeBase && customGridNodeBase.ContainsPoint(pos))
			{
				NNInfoInternal internalInfo = new NNInfoInternal(hint);
				internalInfo.clampedPosition = pos;
				return new NNInfo(internalInfo);
			}
			if (AstarPath.active == null)
			{
				return default(NNInfo);
			}
			return AstarPath.active.GetNearest(pos, constraint, hint);
		}
	}

	public static Vector3 FindClosestPointToStandOn(Vector3 pos, float corpulence, CustomGridNodeBase hint = null)
	{
		NNInfo nearestNode = GetNearestNode(pos, hint);
		CustomGridNode customGridNode = (CustomGridNode)nearestNode.node;
		pos = nearestNode.position;
		for (int i = 0; i < 8; i++)
		{
			if (!customGridNode.HasConnectionInDirection(i))
			{
				Vector3 vector = CustomGraphHelper.GetVector3Direction(i) * 1.35f;
				CustomGridNode customGridNode2 = (CustomGridNode)customGridNode.GetNeighbourAlongDirection(i);
				if (customGridNode2 != null)
				{
					vector = (customGridNode2.Vector3Position - customGridNode.Vector3Position).To2D().To3D();
				}
				float magnitude = vector.magnitude;
				Vector3 vector2 = vector / magnitude;
				Vector3 rhs = (pos - customGridNode.Vector3Position).To2D().To3D();
				float num = Vector3.Dot(vector2, rhs);
				if (num > magnitude / 2f - corpulence)
				{
					pos += vector2 * (magnitude / 2f - corpulence - num);
				}
			}
		}
		return pos;
	}

	public static uint GetArea(Vector3 pos)
	{
		return GetNearestNode(pos).node?.Area ?? 999999;
	}

	private static float NormalizeAngle(float angle)
	{
		float num = (angle + 180f) / 360f;
		return (num - Mathf.Floor(num)) * 360f - 180f;
	}

	private static void SortAngles(ref float startAngle, ref float endAngle)
	{
		startAngle = NormalizeAngle(startAngle);
		endAngle = NormalizeAngle(endAngle);
		if ((startAngle > endAngle && startAngle - endAngle < 180f) || (startAngle < endAngle && endAngle - startAngle > 180f))
		{
			float num = startAngle;
			startAngle = endAngle;
			endAngle = num;
		}
	}

	public static Vector3 GetDeepNavmeshPoint(Vector3 pos)
	{
		return Vector3.MoveTowards(pos, GetNearestNode(pos).position, 0.1f);
	}

	public void DrawDebugPoints(Vector3 p, Vector2 forward)
	{
	}
}
