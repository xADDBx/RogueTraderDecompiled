using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core.Easing;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.View;

[DisallowMultipleComponent]
public class UnitMovementAgentShip : UnitMovementAgentBase
{
	private enum ShipAccelerationState
	{
		Acceleration,
		Normal,
		SlowDown
	}

	[Tooltip("Distance from start of path to end of Acceleration phase in cells\nBetween AccelerationDistance and AccelerationDistanceInCells will be chosen the smallest distance")]
	[Range(0f, 1f)]
	public float AccelerationDistance = 0.33f;

	[Tooltip("Distance from start of path to end of Acceleration phase in relative value (example, 1/3 of path)\nBetween AccelerationDistance and AccelerationDistanceInCells will be chosen the smallest distance")]
	[Range(0f, 100f)]
	public int AccelerationDistanceInCells = 1;

	[Tooltip("Distance from end of path to start of Slowdown phase in relative value (example, 1/3 of path)\nBetween SlowdownDistance and SlowdownDistanceInCells will be chosen the smallest distance")]
	[Range(0f, 1f)]
	public float SlowdownDistance = 0.25f;

	[Tooltip("Distance from end of path to start of Slowdown phase in cells\nBetween SlowdownDistance and SlowdownDistanceInCells will be chosen the smallest distance")]
	[Range(0f, 100f)]
	public int SlowdownDistanceInCells = 1;

	[Tooltip("Easing returns value [0; 1]. This value will be used for remap value\nAcceleration phase: [0; 1] -> [MinAccelerationModifier; 1]\nSlowdown phase: [0; 1] -> [1; MinAccelerationModifier]")]
	[Range(0.01f, 1f)]
	public float MinAccelerationModifier = 0.2f;

	[Tooltip("Easing for Acceleration Phase. Speed *= Easing value. Easing value = [0; 1].\nIMPORTANT: All Elastic, Back, Bounce, Flash easings are not supported")]
	public Ease AccelerationPhaseCurve = Ease.InCubic;

	[Tooltip("Easing for Slowdown Phase. Speed *= Easing value. Easing value = [1; 0]. Value of this easing is re-mapped!\nIMPORTANT: All Elastic, Back, Bounce, Flash easings are not supported")]
	public Ease SlowdownPhaseCurve = Ease.InCubic;

	private List<Vector3> m_ShipWaypoints;

	private ShipAccelerationState m_AccelerationState;

	private float m_DistanceFromStart;

	private float m_EndOfAccelerationPhase;

	private float m_StartOfSlowdownPhase;

	public override Vector3 Position
	{
		get
		{
			if (base.Unit != null)
			{
				return SizePathfindingHelper.FromMechanicsToViewPosition(base.Unit.Data, base.Unit.Data.Position);
			}
			return base.transform.position;
		}
		set
		{
			if (base.Unit != null)
			{
				base.Unit.Data.Position = SizePathfindingHelper.FromViewToMechanicsPosition(base.Unit.Data, value);
			}
			else
			{
				base.transform.position = value;
			}
		}
	}

	private Vector3 UnitPosition
	{
		get
		{
			if (base.Unit != null)
			{
				return base.Unit.Data.Position;
			}
			return base.transform.position;
		}
		set
		{
			if (base.Unit != null)
			{
				base.Unit.Data.Position = value;
			}
			else
			{
				base.transform.position = value;
			}
		}
	}

	private bool OnLastSegment
	{
		get
		{
			if (m_ShipWaypoints != null)
			{
				return m_ShipWaypoints.Count - 1 <= m_NextPointIndex;
			}
			return false;
		}
	}

	private bool OnFirstSegment
	{
		get
		{
			if (m_ShipWaypoints != null && m_ShipWaypoints.Count > 1)
			{
				return m_NextPointIndex == 1;
			}
			return false;
		}
	}

	public override bool WantsToMove
	{
		get
		{
			if (m_ShipWaypoints != null)
			{
				return m_NextPointIndex > 0;
			}
			return false;
		}
	}

	public override bool IsReallyMoving => WantsToMove;

	private bool IsTurn(Vector3 firstWaypoint, Vector3 secondWaypoint, Vector3 thirdWaypoint)
	{
		return Vector2.Angle((secondWaypoint - firstWaypoint).To2D().normalized, (thirdWaypoint - secondWaypoint).To2D().normalized) > 1f;
	}

	private IEnumerable<Vector3> GetTurnWaypoints(Vector3 startPoint, Vector3 centerPoint, Vector3 endPoint, int amountOfPoints)
	{
		float fraction = 1f / (float)amountOfPoints;
		for (int i = 0; i < amountOfPoints; i++)
		{
			float t2 = (float)i * fraction;
			yield return Bezier(t2);
		}
		Vector3 Bezier(float t)
		{
			return (float)Math.Pow(1f - t, 2.0) * startPoint + 2f * t * (1f - t) * centerPoint + (float)Math.Pow(t, 2.0) * endPoint;
		}
	}

	private void GenerateWaypoints(Path path)
	{
		if (path?.vectorPath == null || path.vectorPath.Count == 0)
		{
			PFLog.Default.Error("Failed to generate ship waypoints. Path is null or empty");
			return;
		}
		if (m_ShipWaypoints == null)
		{
			m_ShipWaypoints = new List<Vector3>();
		}
		m_ShipWaypoints.Clear();
		for (int i = 0; i < path.vectorPath.Count; i++)
		{
			Vector3 vector = path.vectorPath[i];
			if (i + 2 >= path.vectorPath.Count)
			{
				m_ShipWaypoints.Add(vector);
				continue;
			}
			Vector3 vector2 = path.vectorPath[i + 1];
			Vector3 vector3 = path.vectorPath[i + 2];
			if (IsTurn(vector, vector2, vector3))
			{
				List<Vector3> vectorPath = path.vectorPath;
				if (vector3 != vectorPath[vectorPath.Count - 1])
				{
					IEnumerable<Vector3> turnWaypoints = GetTurnWaypoints(vector, vector2, vector3, 6);
					m_ShipWaypoints.AddRange(turnWaypoints);
					i++;
					continue;
				}
			}
			m_ShipWaypoints.Add(vector);
		}
	}

	public override void TickMovement(float deltaTime)
	{
		m_NodeLinkTraverser.Tick(deltaTime);
		if (!CanTick())
		{
			return;
		}
		try
		{
			bool firstTick = m_FirstTick;
			if (m_FirstTick)
			{
				_ = m_Velocity.sqrMagnitude < 0.1f;
			}
			else
				_ = 0;
			m_FirstTick = false;
			if (firstTick)
			{
				InitAcceleration();
			}
			bool flag = Game.Instance.Player.UISettings.FastMovement && !CameraRig.Instance.IsScrollingByRoutineSynced && Game.Instance.Player.IsInCombat && Mathf.Abs(Game.Instance.TimeController.SlowMoTimeScale - 1f) < 0.01f;
			Vector2 vector = UnitPosition.To2D();
			if (firstTick && m_HasTail)
			{
				m_TailPosition = UnitPosition + 2f * -base.Unit.Data.Forward;
			}
			bool isPositionChanged = (vector - m_PreviousPosition).sqrMagnitude > 1E-08f;
			m_PreviousPosition = vector;
			float distanceToWaypoint = Vector2.Distance(m_NextWaypoint, vector);
			TrySkipWaypoints(ref distanceToWaypoint, deltaTime);
			Vector2 vector2 = (m_HasTail ? (m_NextWaypoint - vector).normalized : base.MoveDirection);
			float maxSpeed = base.MaxSpeed;
			EstimatedTimeLeft = Mathf.Max(0f, (m_RemainingPathDistance + distanceToWaypoint) / base.MaxSpeed);
			Vector2 vector3 = (m_NextWaypoint - vector) / Mathf.Max(distanceToWaypoint, 0.0001f);
			float desiredSpeed = maxSpeed;
			UpdateAccelerationState(firstTick);
			UpdateSpeedModifiers(deltaTime);
			float num = Vector2.SignedAngle(vector2, vector3);
			float num2 = Math.Abs(num);
			bool flag2 = base.Unit != null && m_HasTail && Vector2.Angle(base.MoveDirection, vector3) > 5f;
			if (num2 > deltaTime * m_AngularSpeed || flag2)
			{
				SlowDown(ref desiredSpeed);
			}
			desiredSpeed = Math.Max(m_CurrentSlowDownCoefficient * desiredSpeed, m_MinSpeed);
			vector2 = CalculateForwardDirection(vector2, vector3, num2, deltaTime, num, distanceToWaypoint, maxSpeed);
			m_Speed = desiredSpeed;
			m_NextVelocity = vector2.To3D() * m_Speed;
			if (flag)
			{
				base.Unit.Data.Translocate(m_NextWaypoint.To3D(), null);
			}
			else
			{
				UnitPosition = MoveInternal(UnitPosition, m_NextVelocity * deltaTime, base.Corpulence);
			}
			if (m_HasTail)
			{
				m_TailPosition = MoveTail(deltaTime, flag);
			}
			Vector2 vector4 = UnitPosition.To2D();
			m_DistanceFromStart += Vector2.Distance(m_PreviousPosition, vector4);
			base.MoveDirection = (m_HasTail ? (UnitPosition - m_TailPosition).To2D().normalized : vector2.normalized);
			Vector2 nextWaypoint2D = GetNextWaypoint2D(m_NextPointIndex - 1);
			Vector2 lhs = m_NextWaypoint - nextWaypoint2D;
			Vector2 rhs = m_NextWaypoint - vector;
			bool flag3 = distanceToWaypoint <= 0.0001f || Vector2.Dot(lhs, rhs) < 0f;
			if (OnLastSegment)
			{
				if (flag)
				{
					base.Unit.Data.SetOrientation(Quaternion.LookRotation(base.MoveDirection.To3D()).eulerAngles.y);
				}
				float num3 = Vector2.Distance(vector4, m_NextWaypoint);
				flag3 = flag3 || num3 < 0.05f;
			}
			if (flag3)
			{
				if (OnLastSegment)
				{
					CompleteMovement(interrupted: false);
				}
				else
				{
					SetWaypoint(m_NextPointIndex + 1);
				}
			}
			if (OnLastSegment && (double)(vector4 - m_NextWaypoint).magnitude < 0.6 && (vector4 - vector).magnitude < 0.0001f)
			{
				CompleteMovement(interrupted: false);
			}
			UpdateStuck(distanceToWaypoint, deltaTime, isPositionChanged);
		}
		finally
		{
		}
	}

	private bool CanTick()
	{
		if (m_NodeLinkTraverser.LastState != 0)
		{
			m_Speed = 0f;
			return false;
		}
		if (base.Unit != null)
		{
			bool isCommandsPreventMovement = base.Unit.IsCommandsPreventMovement;
			bool flag = base.Unit.AnimationManager != null && (base.Unit.AnimationManager.IsPreventingMovement || base.Unit.AnimationManager.IsGoingCover);
			if (isCommandsPreventMovement || flag)
			{
				return false;
			}
		}
		if (!IsReallyMoving)
		{
			return false;
		}
		return true;
	}

	private void UpdateSpeedModifiers(float deltaTime)
	{
		if (m_AccelerationState == ShipAccelerationState.Normal)
		{
			if (m_SlowDownTime > 0f)
			{
				float t = ((m_Acceleration * deltaTime < m_CurrentSlowDownCoefficient - m_SlowDownCoefficient) ? (m_Acceleration * deltaTime / (m_CurrentSlowDownCoefficient - m_SlowDownCoefficient)) : 1f);
				m_CurrentSlowDownCoefficient = Mathf.Max(Mathf.Lerp(m_CurrentSlowDownCoefficient, m_SlowDownCoefficient, t), m_SlowDownCoefficient);
				if (m_CurrentSlowDownCoefficient < m_SlowDownCoefficient + 0.01f)
				{
					m_SlowDownTime -= deltaTime;
				}
			}
			else if (m_CurrentSlowDownCoefficient < 0.99f)
			{
				float t2 = ((m_Acceleration * deltaTime < 1f - m_CurrentSlowDownCoefficient) ? (m_Acceleration * deltaTime / (1f - m_CurrentSlowDownCoefficient)) : 1f);
				m_CurrentSlowDownCoefficient = Mathf.Max(Mathf.Lerp(m_CurrentSlowDownCoefficient, 1f, t2), m_SlowDownCoefficient);
			}
		}
		if (m_AccelerationState == ShipAccelerationState.Acceleration)
		{
			float x = EaseManager.ToEaseFunction(AccelerationPhaseCurve)(m_DistanceFromStart, m_EndOfAccelerationPhase, 0f, 0f);
			m_CurrentSlowDownCoefficient = math.remap(0f, 1f, MinAccelerationModifier, 1f, x);
		}
		else if (m_AccelerationState == ShipAccelerationState.SlowDown)
		{
			float num = DistanceBetweenWaypoints(0, m_ShipWaypoints.Count - 1);
			float x2 = EaseManager.ToEaseFunction(SlowdownPhaseCurve)(m_DistanceFromStart - m_StartOfSlowdownPhase, num - m_StartOfSlowdownPhase, 0f, 0f);
			m_CurrentSlowDownCoefficient = math.remap(0f, 1f, 1f, MinAccelerationModifier, x2);
		}
	}

	private Vector2 CalculateForwardDirection(Vector2 fwd, Vector2 desiredDir, float angle, float deltaTime, float signedAngle, float distance, float maxSpeed)
	{
		float num = m_CombatAngularSpeed;
		float num2 = distance / Mathf.Max(maxSpeed, 0.01f);
		if (OnLastSegment && Vector3.Angle(fwd, desiredDir) / num * 2f > num2 && angle / num2 > num)
		{
			num = angle / num2;
		}
		return (Quaternion.AngleAxis(Mathf.Min(num * deltaTime, angle), Vector3.down * Math.Sign(signedAngle)) * fwd.To3D()).To2D();
	}

	private void TrySkipWaypoints(ref float distanceToWaypoint, float deltaTime)
	{
		while (!OnLastSegment && distanceToWaypoint < m_Speed * deltaTime)
		{
			Vector2 nextWaypoint = m_NextWaypoint;
			SetWaypoint(m_NextPointIndex + 1);
			distanceToWaypoint += Vector2.Distance(m_NextWaypoint, nextWaypoint);
			m_StuckTimeStop = 0f;
		}
	}

	private void UpdateStuck(float distanceToWaypoint, float deltaTime, bool isPositionChanged)
	{
		if (distanceToWaypoint < m_MinWaypointDistance || isPositionChanged)
		{
			m_MinWaypointDistance = distanceToWaypoint;
			m_StuckTimeStop = 0f;
			m_StuckTimeDirection = 0f;
		}
		m_StuckTimeStop += deltaTime / Game.Instance.TimeController.GameTimeScale;
		m_StuckTimeDirection += deltaTime / Game.Instance.TimeController.GameTimeScale;
		if (m_StuckTimeDirection > 0.15f)
		{
			m_LastTurnAngle = 0f;
			m_StuckTimeDirection = 0f;
		}
		if (m_StuckTimeStop > 1f)
		{
			if (base.Unit != null)
			{
				base.Unit.Data.Translocate(m_NextWaypoint.To3D(), null);
			}
			else
			{
				UnitPosition = m_NextWaypoint.To3D();
			}
			if (OnLastSegment)
			{
				CompleteMovement(interrupted: false);
			}
			else
			{
				SetWaypoint(m_NextPointIndex + 1);
			}
			m_StuckTimeStop = 0f;
		}
	}

	private Vector3 MoveTail(float deltaTime, bool moveUsingTranslocate)
	{
		Vector3 nextWaypoint3D = GetNextWaypoint3D(m_NextPointIndex - 1);
		Vector3 normalized;
		if (OnLastSegment)
		{
			Vector3 nextWaypoint3D2 = GetNextWaypoint3D(m_ShipWaypoints.Count - 1);
			List<Vector3> shipWaypoints = m_ShipWaypoints;
			Vector3 vector = shipWaypoints[shipWaypoints.Count - 1];
			List<Vector3> shipWaypoints2 = m_ShipWaypoints;
			normalized = (nextWaypoint3D2 + 2f * -(vector - shipWaypoints2[shipWaypoints2.Count - 2]).normalized - m_TailPosition).normalized;
		}
		else
		{
			normalized = (nextWaypoint3D - m_TailPosition).normalized;
		}
		Vector3 shift = normalized * (m_Speed * deltaTime * 0.25f);
		IntRect sizeRect = base.Unit.Data.SizeRect;
		float num = (float)((sizeRect.Height - sizeRect.Width) * (sizeRect.Height - sizeRect.Width)) * GraphParamsMechanicsCache.GridCellSize * GraphParamsMechanicsCache.GridCellSize;
		int num2 = (moveUsingTranslocate ? 100 : 5);
		int num3 = 0;
		Vector3 vector2 = m_TailPosition;
		while ((UnitPosition - vector2).sqrMagnitude > num && Vector3.Dot(nextWaypoint3D - vector2, normalized) >= 0f && num3 < num2)
		{
			vector2 = MoveInternal(vector2, shift, base.Corpulence);
			num3++;
		}
		return vector2;
	}

	private new void SetWaypoint(int index)
	{
		if (base.Path == null)
		{
			PFLog.Default.Warning("No path");
			return;
		}
		if (index <= 0 || index >= m_ShipWaypoints.Count)
		{
			PFLog.Default.Warning("Invalid path point {0}", index);
			return;
		}
		if (m_ShipWaypoints.TryGet(index, out var element) && Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && AstarPath.active != null)
		{
			GraphNode node = AstarPath.active.GetNearest(element).node;
			if (WarhammerBlockManager.Instance == null || WarhammerBlockManager.Instance.NodeContainsInvisibleAnyExcept(node, m_Blocker))
			{
				CompleteMovement(interrupted: true);
				return;
			}
		}
		m_NextPointIndex = index;
		m_NextWaypoint = GetNextWaypoint2D(index);
		m_RemainingPathDistance = 0f;
		for (int i = index; i < m_ShipWaypoints.Count - 1; i++)
		{
			Vector2 nextWaypoint2D = GetNextWaypoint2D(i);
			Vector2 nextWaypoint2D2 = GetNextWaypoint2D(i + 1);
			m_RemainingPathDistance += Vector2.Distance(nextWaypoint2D, nextWaypoint2D2);
		}
		if (m_Destination.HasValue)
		{
			List<Vector3> shipWaypoints = m_ShipWaypoints;
			Vector3 v = shipWaypoints[shipWaypoints.Count - 1];
			m_RemainingPathDistance += Vector2.Distance(v.To2D(), m_Destination.Value.To2D());
		}
		m_RemainingPathDistance -= base.ApproachRadius;
		if (base.Unit != null)
		{
			base.Unit.OnMovementWaypointUpdate(index);
		}
		ResetStuck();
	}

	protected override Vector2 GetNextWaypoint2D(int index)
	{
		return GetNextWaypoint3D(index).To2D();
	}

	protected override Vector3 GetNextWaypoint3D(int index)
	{
		if (index < 0 || index > m_ShipWaypoints.Count)
		{
			PFLog.Default.Error("Trying to access waypoint out of range");
			return Vector3.zero;
		}
		return m_ShipWaypoints[index];
	}

	protected override void StartMovingWithPath(ForcedPath p, bool forcedPath, bool requestedNewPath)
	{
		GenerateWaypoints(p);
		base.StartMovingWithPath(p, forcedPath, requestedNewPath);
	}

	private float DistanceBetweenWaypoints(int startWaypointIndex, int endWaypointIndex, float defaultValue = 0f)
	{
		if (startWaypointIndex < 0 || endWaypointIndex < 0 || startWaypointIndex >= m_ShipWaypoints.Count || endWaypointIndex >= m_ShipWaypoints.Count)
		{
			return defaultValue;
		}
		if (startWaypointIndex >= endWaypointIndex)
		{
			return defaultValue;
		}
		float num = 0f;
		for (int i = startWaypointIndex; i != endWaypointIndex; i++)
		{
			num += Vector3.Distance(GetNextWaypoint3D(i), GetNextWaypoint3D(i + 1));
		}
		return num;
	}

	private void UpdateAccelerationState(bool isMovementStart)
	{
		if (isMovementStart)
		{
			m_AccelerationState = ShipAccelerationState.Acceleration;
		}
		else if (m_AccelerationState == ShipAccelerationState.Acceleration)
		{
			if (m_DistanceFromStart >= m_EndOfAccelerationPhase)
			{
				m_AccelerationState = ShipAccelerationState.Normal;
			}
		}
		else if (m_AccelerationState == ShipAccelerationState.Normal && m_DistanceFromStart >= m_StartOfSlowdownPhase)
		{
			m_AccelerationState = ShipAccelerationState.SlowDown;
		}
	}

	private void InitAcceleration()
	{
		m_AccelerationState = ShipAccelerationState.Acceleration;
		m_DistanceFromStart = 0.001f;
		int count = m_ShipWaypoints.Count;
		float num = DistanceBetweenWaypoints(0, count - 1);
		float a = num * AccelerationDistance;
		float b = DistanceBetweenWaypoints(0, AccelerationDistanceInCells, 100f);
		m_EndOfAccelerationPhase = Mathf.Min(a, b);
		if (base.IsCharging)
		{
			m_StartOfSlowdownPhase = float.MaxValue;
			return;
		}
		float a2 = num - num * SlowdownDistance;
		float b2 = num - DistanceBetweenWaypoints(count - SlowdownDistanceInCells - 1, count - 1);
		m_StartOfSlowdownPhase = Mathf.Min(a2, b2);
	}

	private void OnDrawGizmos()
	{
		if (IsReallyMoving)
		{
			Debug.DrawLine(m_NextWaypoint.To3D(), m_NextWaypoint.To3D() + Vector3.up, Color.red);
			Debug.DrawLine(GetNextWaypoint3D(m_NextPointIndex - 1), GetNextWaypoint3D(m_NextPointIndex - 1) + Vector3.up, Color.red);
			for (int i = 0; i < m_ShipWaypoints.Count - 1; i++)
			{
				Color color = ((i < m_NextPointIndex) ? Color.green : Color.cyan);
				Vector3 nextWaypoint3D = GetNextWaypoint3D(i);
				Vector3 nextWaypoint3D2 = GetNextWaypoint3D(i + 1);
				Debug.DrawLine(nextWaypoint3D, nextWaypoint3D2, Color.magenta, 1f);
				Debug.DrawLine(nextWaypoint3D, nextWaypoint3D + Vector3.up, color, 1f);
			}
			Debug.DrawLine(GetNextWaypoint3D(m_ShipWaypoints.Count - 1), GetNextWaypoint3D(m_ShipWaypoints.Count - 1) + Vector3.up, Color.green, 1f);
		}
	}
}
