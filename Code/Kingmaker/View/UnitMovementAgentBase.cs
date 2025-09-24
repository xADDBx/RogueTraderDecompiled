using System;
using System.Collections.Generic;
using Core.Cheats;
using JetBrains.Annotations;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.Controllers.Optimization;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.CountingGuard;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View;

public class UnitMovementAgentBase : MonoBehaviour, IEntitySubscriber, IUnitLifeStateChanged<EntitySubscriber>, IUnitLifeStateChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitLifeStateChanged, EntitySubscriber>
{
	private GameObject m_Owner;

	[SerializeField]
	protected bool m_UseAcceleration;

	public const float DefaultAcceleration = 20f;

	[SerializeField]
	[ShowIf("m_UseAcceleration")]
	public float m_Acceleration = 20f;

	[SerializeField]
	protected float m_MinSpeed = 0.2f;

	public const float DefaultAngularSpeed = 360f;

	[SerializeField]
	public float m_AngularSpeed = 360f;

	[SerializeField]
	protected float m_CombatAngularSpeed = 720f;

	[SerializeField]
	protected float m_SlowDownCoefficient = 0.7f;

	[SerializeField]
	private bool m_AvoidanceDisabled;

	private readonly CountingGuard m_AvoidanceDisabledCounter = new CountingGuard();

	protected PathfindingService.Options m_TurnBasedOptions;

	protected PathfindingService.Options m_RealTimeOptions;

	protected WarhammerSingleNodeBlocker m_Blocker;

	protected WarhammerTraversalProvider m_TraversalProvider;

	protected WarhammerNodeLinkTraverser m_NodeLinkTraverser;

	[CanBeNull]
	private ForcedPath m_Path;

	protected bool m_RequestedNewPath;

	private bool m_Roaming;

	protected int m_NextPointIndex;

	protected float m_RemainingPathDistance;

	internal float EstimatedTimeLeft;

	protected float m_Speed;

	protected Vector2 m_NextWaypoint;

	protected Vector3? m_Destination;

	private Vector3? m_NoPathDestination;

	protected float m_LastTurnAngle;

	internal static readonly List<UnitMovementAgentBase> AllAgents = new List<UnitMovementAgentBase>();

	[NonSerialized]
	[CanBeNull]
	public List<UnitMovementAgentBase> ObstaclesGroup;

	[NonSerialized]
	[CanBeNull]
	public List<UnitMovementAgentBase> UnitContacts;

	protected const float StuckTimeStop = 1f;

	protected const float StuckTimeResetDirection = 0.15f;

	protected float m_StuckTimeStop;

	protected float m_StuckTimeDirection;

	protected float m_MinWaypointDistance;

	protected Vector2 m_PreviousPosition;

	protected const float InitialSlowDownTimeMin = 0.2f;

	protected const float InitialSlowDownTimeMax = 0.3f;

	protected const float FadeSlowDownTime = 0.1f;

	private static readonly Vector3 s_DebugShift = Vector3.up * 0.05f;

	private static readonly float s_IgnoreAngleCos = Mathf.Cos(MathF.PI / 6f);

	protected float m_SlowDownTime;

	protected bool m_FirstTick;

	protected float m_CurrentSlowDownCoefficient = 1f;

	protected bool m_IsInForceMode;

	private int m_ChargingCounter;

	private TimeSpan m_ChargeAvoidanceFinishTime;

	public const float DefaultCorpulenceInPlayerParty = 2f;

	private int m_FirstTickCounter;

	public bool IsPositionChanged;

	protected Vector3 m_Velocity;

	protected Vector3 m_NextVelocity;

	protected bool m_HasTail;

	protected Vector3 m_TailPosition;

	private readonly float m_OffsetForLastNode = 0.4f;

	private static readonly Vector3[][] m_NeighboursRectOffset = new Vector3[8][]
	{
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2],
		new Vector3[2]
	};

	private static readonly Vector3[] m_OffsetDirection = new Vector3[8];

	[Cheat(Name = "movement_use_raycast")]
	public static bool FallbackToRayCast { get; set; }

	public PathfindingService.Options RealTimeOptions => m_RealTimeOptions;

	public PathfindingService.Options TurnBasedOptions => m_TurnBasedOptions;

	public WarhammerSingleNodeBlocker Blocker => m_Blocker;

	public ITraversalProvider TraversalProvider
	{
		get
		{
			m_TraversalProvider.SetIsPlayerEnemy((Unit?.Data?.IsPlayerEnemy).GetValueOrDefault());
			return m_TraversalProvider;
		}
	}

	public WarhammerNodeLinkTraverser NodeLinkTraverser => m_NodeLinkTraverser;

	public bool IsInNodeLinkQueue => m_NodeLinkTraverser?.IsInQueue ?? false;

	public bool IsTraverseInProgress => m_NodeLinkTraverser.LastState != WarhammerNodeLinkTraverser.State.None;

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

	public float RemainingDistance => m_RemainingPathDistance + Vector2.Distance(m_NextWaypoint, m_PreviousPosition);

	public float AngleToNextWaypoint => Vector2.Angle(MoveDirection, m_NextWaypoint - m_PreviousPosition);

	public virtual Vector2 NextWaypoint => m_NextWaypoint;

	public bool ConnectedToObstacles { get; set; }

	public bool FirstTick => m_FirstTick;

	public Vector2 MoveDirection { get; protected set; }

	public virtual Vector3 FinalDirection { get; protected set; }

	public bool IsStopping { get; protected set; }

	public float? MaxSpeedOverride { get; set; }

	public bool ForceRoaming { get; set; }

	public float SpeedIndicator => m_Speed / MaxSpeed;

	public virtual bool AvoidanceDisabled
	{
		get
		{
			if (!m_AvoidanceDisabled && !m_AvoidanceDisabledCounter)
			{
				if (Unit != null)
				{
					if (Unit.EntityData.LifeState.IsConscious)
					{
						return Unit.EntityData.IsInCombat;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		set
		{
			m_AvoidanceDisabledCounter.Value = value;
		}
	}

	public bool IsCharging
	{
		get
		{
			return m_ChargingCounter > 0;
		}
		set
		{
			if (value)
			{
				m_ChargingCounter++;
				return;
			}
			m_ChargingCounter--;
			if (m_ChargingCounter <= 0)
			{
				m_ChargeAvoidanceFinishTime = Game.Instance.TimeController.GameTime + 1.Seconds();
			}
		}
	}

	private bool ChargingAvoidance
	{
		get
		{
			if (IsCharging)
			{
				return true;
			}
			if (Game.Instance.TimeController.GameTime < m_ChargeAvoidanceFinishTime)
			{
				return true;
			}
			return false;
		}
	}

	public float Corpulence { get; private set; }

	public float ApproachRadius { get; private set; }

	protected bool CombatMode
	{
		get
		{
			if (Unit != null && Unit.EntityData != null)
			{
				return Unit.EntityData.IsInCombat;
			}
			return false;
		}
	}

	private bool OnFirstSegment
	{
		get
		{
			if (Path != null && Path.vectorPath.Count > 1)
			{
				return m_NextPointIndex == 1;
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
				return Path.vectorPath.Count - 1 <= m_NextPointIndex;
			}
			return false;
		}
	}

	public virtual bool WantsToMove => Path != null;

	public virtual bool IsReallyMoving
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

	public bool PathFailed
	{
		get
		{
			if (Path != null)
			{
				if (!Path.error)
				{
					return m_NextPointIndex == 0;
				}
				return true;
			}
			return false;
		}
	}

	public float MaxSpeed
	{
		get
		{
			float? maxSpeedOverride = MaxSpeedOverride;
			if (!maxSpeedOverride.HasValue)
			{
				if (!(Unit != null) || Unit.EntityData == null)
				{
					return 30.Feet().Meters / 3f;
				}
				return Unit.EntityData.Movable.CurrentSpeedMps;
			}
			return maxSpeedOverride.GetValueOrDefault();
		}
	}

	public float Speed => m_Speed;

	[CanBeNull]
	public AbstractUnitEntityView Unit { get; private set; }

	public virtual Vector3 Position
	{
		get
		{
			if (Unit != null)
			{
				return Unit.Data.Position;
			}
			return base.transform.position;
		}
		set
		{
			if (Unit != null)
			{
				Unit.Data.Position = value;
			}
			else
			{
				base.transform.position = value;
			}
		}
	}

	public IEntity GetSubscribingEntity()
	{
		return Unit.Data;
	}

	private void InitTurnBasedOptions()
	{
		m_TurnBasedOptions = new PathfindingService.Options
		{
			Modifiers = new IPathModifier[1]
			{
				new StartEndModifier
				{
					exactStartPoint = StartEndModifier.Exactness.SnapToNode,
					exactEndPoint = StartEndModifier.Exactness.SnapToNode
				}
			}
		};
		ResetBlocker();
	}

	public void ResetBlocker()
	{
		if (Unit != null)
		{
			m_Blocker?.Unblock();
			m_Blocker = new WarhammerSingleNodeBlocker(Unit.Data);
			m_TraversalProvider = CreateTraversalProvider(Unit, m_Blocker);
		}
		UpdateBlocker();
	}

	private WarhammerTraversalProvider CreateTraversalProvider([NotNull] AbstractUnitEntityView unit, [NotNull] WarhammerSingleNodeBlocker blocker)
	{
		return new WarhammerTraversalProvider(blocker, unit.Data.SizeRect, unit.Data.IsPlayerEnemy);
	}

	private void InitRealTimeOptions()
	{
		if (Unit != null)
		{
			float num = (Unit.Data.IsPlayerFaction ? 2f : ((float)(Unit.Data.SizeRect.Width + Unit.Data.SizeRect.Height)));
			num = num / 4f * GraphParamsMechanicsCache.GridCellSize * 0.55f;
			Corpulence = num;
		}
		else
		{
			Corpulence = 1f;
		}
		m_RealTimeOptions = new PathfindingService.Options
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
					shrink = Corpulence * 1.1f
				}
			}
		};
	}

	public void Init([NotNull] GameObject owner)
	{
		if ((bool)m_Owner)
		{
			throw new InvalidOperationException("CharacterAgent already initialized");
		}
		m_Owner = owner;
		Unit = m_Owner.GetComponent<AbstractUnitEntityView>();
		m_NodeLinkTraverser = new WarhammerNodeLinkTraverser(this);
		InitRealTimeOptions();
		InitTurnBasedOptions();
		InitMoveDirection();
		IntRect? intRect = Unit?.Data?.SizeRect;
		int hasTail;
		if (intRect.HasValue)
		{
			IntRect valueOrDefault = intRect.GetValueOrDefault();
			hasTail = ((valueOrDefault.Width != valueOrDefault.Height) ? 1 : 0);
		}
		else
		{
			hasTail = 0;
		}
		m_HasTail = (byte)hasTail != 0;
	}

	public void Tick()
	{
	}

	[UsedImplicitly]
	private void OnEnable()
	{
		if (!ContextData<UnitHelper.UnitHologram>.Current)
		{
			AllAgents.Add(this);
			UpdateBlocker();
		}
	}

	[UsedImplicitly]
	private void OnDisable()
	{
		if (!ContextData<UnitHelper.UnitHologram>.Current)
		{
			AllAgents.Remove(this);
			ObstaclesHelper.RemoveFromGroup(this);
			m_Blocker?.Unblock();
		}
	}

	[UsedImplicitly]
	private void OnDestroy()
	{
		Path = null;
	}

	public void ForcePath([NotNull] ForcedPath p, bool disableApproachRadius = false)
	{
		ApproachRadius = (disableApproachRadius ? 0.05f : 0.3f);
		List<Vector3> vectorPath = p.vectorPath;
		m_Destination = vectorPath[vectorPath.Count - 1];
		StartMovingWithPath(p, forcedPath: true, requestedNewPath: true);
	}

	public void FollowPath(ForcedPath p, Vector3 destination, float approachRadius)
	{
		bool num = m_Destination.HasValue && destination == m_Destination.Value;
		bool flag = m_NoPathDestination.HasValue && destination == m_NoPathDestination.Value;
		bool requestedNewPath = !num && !flag;
		ApproachRadius = approachRadius;
		m_Destination = destination;
		InitMoveDirection();
		if (ForceRoaming)
		{
			m_Roaming = true;
		}
		else
		{
			m_Roaming = (Unit?.EntityData.Commands.CurrentMoveTo)?.Roaming ?? false;
		}
		StartMovingWithPath(p, forcedPath: false, requestedNewPath);
	}

	public static HashSet<GraphNode> CacheThreateningAreaCells(AbstractUnitEntity entity)
	{
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return hashSet;
		}
		foreach (UnitGroupMemory.UnitInfo enemy in baseUnitEntity.CombatGroup.Memory.Enemies)
		{
			BaseUnitEntity unit = enemy.Unit;
			if (unit.CanMakeAttackOfOpportunity(baseUnitEntity))
			{
				hashSet.UnionWith(unit.GetThreateningArea());
			}
		}
		return hashSet;
	}

	public virtual void TickMovement(float deltaTime)
	{
		m_NodeLinkTraverser.Tick(deltaTime);
		if (IsTraverseInProgress)
		{
			m_Speed = 0f;
		}
		else
		{
			if (Unit != null && (Unit.IsCommandsPreventMovement || (Unit.AnimationManager != null && (Unit.AnimationManager.IsPreventingMovement || Unit.AnimationManager.IsGoingCover))))
			{
				return;
			}
			try
			{
				if (!IsReallyMoving)
				{
					return;
				}
				bool firstTick = m_FirstTick;
				bool flag = m_FirstTick && m_Velocity.sqrMagnitude < 0.1f;
				if (m_FirstTick)
				{
					m_FirstTickCounter = 0;
				}
				else
				{
					m_FirstTickCounter++;
				}
				m_FirstTick = false;
				bool flag2 = Game.Instance.Player.UISettings.FastMovement && Game.Instance.CurrentMode != GameModeType.Cutscene && m_FirstTickCounter >= 2 && !CameraRig.Instance.IsScrollingByRoutineSynced && Game.Instance.Player.IsInCombat && Mathf.Abs(Game.Instance.TimeController.SlowMoTimeScale - 1f) < 0.01f;
				Vector2 vector = Position.To2D();
				if (m_HasTail)
				{
					vector -= SizePathfindingHelper.GetSizePositionOffset(Unit.Data.SizeRect, Unit.Data.Forward, shiftRight: false).To2D();
				}
				Vector2 vector2 = (m_HasTail ? (m_NextWaypoint - vector).normalized : MoveDirection);
				float num = Vector2.Distance(m_NextWaypoint, vector);
				if (firstTick && m_HasTail)
				{
					m_TailPosition = Position + 2f * SizePathfindingHelper.GetSizePositionOffset(Unit.Data.SizeRect, Unit.Data.Forward, shiftRight: false);
				}
				bool flag3 = (IsPositionChanged = (vector - m_PreviousPosition).sqrMagnitude > 1E-08f);
				m_PreviousPosition = vector;
				while (!OnLastSegment && num < m_Speed * deltaTime)
				{
					Vector2 nextWaypoint = m_NextWaypoint;
					SetWaypoint(m_NextPointIndex + 1);
					num += Vector2.Distance(m_NextWaypoint, nextWaypoint);
					m_StuckTimeStop = 0f;
				}
				float num2 = ((Unit != null && Unit.Data.IsInFogOfWar && Game.Instance.TurnController.TurnBasedModeActive) ? (MaxSpeed * 8f) : MaxSpeed);
				float num3 = Mathf.Min(m_MinSpeed, num2);
				EstimatedTimeLeft = Mathf.Max(0f, (m_RemainingPathDistance + num) / MaxSpeed);
				float num4 = num / Mathf.Max(num2, 0.01f);
				Vector2 vector3 = (m_NextWaypoint - vector) / Mathf.Max(num, 0.0001f);
				Vector2 desiredDir = vector3;
				float desiredSpeed = num2;
				if (m_UseAcceleration)
				{
					if (OnFirstSegment)
					{
						float magnitude = (m_NextWaypoint - Path.vectorPath[m_NextPointIndex - 1].To2D()).magnitude;
						float num5 = Mathf.Clamp(num2 * num2 / (2f * m_Acceleration), 0f, magnitude);
						desiredSpeed = Mathf.Lerp(num3, num2, (magnitude - num) / num5);
					}
					if (OnLastSegment)
					{
						float magnitude2 = (m_NextWaypoint - Path.vectorPath[m_NextPointIndex - 1].To2D()).magnitude;
						float num6 = Mathf.Clamp(num2 * num2 / (2f * m_Acceleration), 0f, magnitude2);
						desiredSpeed = Mathf.Min(desiredSpeed, Mathf.Lerp(num2, num3, (magnitude2 - num) / num6));
					}
				}
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
				UpdateAvoidance(ref desiredDir, ref desiredSpeed, out var intersectsObstacles, out var hasNavmeshObstacles);
				float value = Vector2.SignedAngle(vector2, desiredDir);
				float num7 = Math.Abs(value);
				bool flag4 = Unit != null && m_HasTail && Vector2.Angle(MoveDirection, desiredDir) > 5f;
				if (num7 > deltaTime * m_AngularSpeed || flag4)
				{
					SlowDown(ref desiredSpeed);
				}
				desiredSpeed *= m_CurrentSlowDownCoefficient;
				if ((firstTick && hasNavmeshObstacles) || (flag && CombatMode) || m_IsInForceMode || (Unit != null && Unit.Data.IsInFogOfWar))
				{
					vector2 = desiredDir;
				}
				else
				{
					float num8 = (CombatMode ? m_CombatAngularSpeed : m_AngularSpeed);
					if (OnLastSegment)
					{
						float num9 = Vector3.Angle(vector2, desiredDir) / num8;
						if (num9 * 2f > num4)
						{
							num8 *= 2f * (num9 / num4);
						}
					}
					vector2 = (Quaternion.AngleAxis(Mathf.Min(num8 * deltaTime, num7), Vector3.down * Math.Sign(value)) * vector2.To3D()).To2D();
				}
				m_Speed = desiredSpeed;
				m_NextVelocity = vector2.To3D() * m_Speed;
				if (flag2)
				{
					Unit.Data.Translocate(m_NextWaypoint.To3D(), null);
					Position = Unit.Data.Position;
				}
				else
				{
					Position = MoveInternal(Position, m_NextVelocity * deltaTime, Corpulence);
				}
				if (m_HasTail)
				{
					m_TailPosition = MoveTail(deltaTime);
				}
				Vector2 vector4 = Position.To2D();
				MoveDirection = (m_HasTail ? (Position - m_TailPosition).To2D().normalized : vector2.normalized);
				Vector2 vector5 = Path.vectorPath[m_NextPointIndex - 1].To2D();
				Vector2 vector6 = m_NextWaypoint - vector5;
				Vector2 rhs = m_NextWaypoint - vector;
				bool flag5 = num <= 0.0001f || vector6.IsApproximatelyZero() || Vector2.Dot(vector6, rhs) < 0f;
				if (OnLastSegment)
				{
					if (flag2)
					{
						Unit.Data.SetOrientation(Quaternion.LookRotation(vector3.To3D()).eulerAngles.y);
					}
					float sqrMagnitude = (vector4 - m_NextWaypoint).sqrMagnitude;
					flag5 = flag5 || sqrMagnitude < 0.0025f;
					flag5 |= m_Roaming && intersectsObstacles && sqrMagnitude <= Corpulence * Corpulence * 4f;
				}
				if (flag5)
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
				if (OnLastSegment)
				{
					float sqrMagnitude2 = (vector4 - m_NextWaypoint).sqrMagnitude;
					float sqrMagnitude3 = (vector - m_NextWaypoint).sqrMagnitude;
					float num10 = Mathf.Max(ApproachRadius, 0.05f);
					bool num11 = sqrMagnitude2 < num10 * num10;
					bool flag6 = (vector4 - vector).sqrMagnitude < 1E-08f;
					bool flag7 = sqrMagnitude3 < sqrMagnitude2;
					if (num11 && (flag6 || flag7))
					{
						CompleteMovement(interrupted: false);
					}
				}
				if (num < m_MinWaypointDistance || flag3)
				{
					m_MinWaypointDistance = num;
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
					if (Unit != null)
					{
						Unit.Data.Translocate(m_NextWaypoint.To3D(), null);
					}
					else
					{
						Position = m_NextWaypoint.To3D();
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
			finally
			{
			}
		}
	}

	private Vector3 MoveTail(float deltaTime)
	{
		Vector3 vector = Path.vectorPath[m_NextPointIndex - 1];
		Vector3 normalized = (vector - m_TailPosition).normalized;
		Vector3 shift = normalized * (m_Speed * deltaTime * 0.25f);
		IntRect sizeRect = Unit.Data.SizeRect;
		float num = (float)((sizeRect.Height - sizeRect.Width) * (sizeRect.Height - sizeRect.Width)) * GraphParamsMechanicsCache.GridCellSize * GraphParamsMechanicsCache.GridCellSize;
		int num2 = 0;
		Vector3 vector2 = m_TailPosition;
		while ((Position - vector2).sqrMagnitude > num && Vector3.Dot(vector - vector2, normalized) >= 0f && num2 < 5)
		{
			vector2 = MoveInternal(vector2, shift, Corpulence);
			num2++;
		}
		return vector2;
	}

	protected virtual Vector3 MoveInternal(Vector3 currentPos, Vector3 shift, float movementCorpulence)
	{
		CustomGridNodeBase targetNode;
		return Move(currentPos, shift, movementCorpulence, out targetNode);
	}

	public static Vector3 Move(Vector3 currentPos, Vector3 shift, float movementCorpulence, out CustomGridNodeBase targetNode)
	{
		using (ProfileScope.NewScope("Move"))
		{
			NNInfo nearestNode = ObstacleAnalyzer.GetNearestNode(currentPos);
			GraphNode node = nearestNode.node;
			currentPos = nearestNode.position;
			Vector3 end = currentPos + shift;
			Linecast.LinecastGrid(node.Graph, currentPos, end, node, out var hit, ObstacleAnalyzer.DefaultXZConstraint, ref Linecast.HasConnectionTransition.Instance);
			end = hit.point;
			targetNode = (CustomGridNodeBase)hit.node;
			if (Game.Instance.CurrentMode == GameModeType.Default && !Game.Instance.TurnController.TurnBasedModeActive)
			{
				for (int i = 0; i < 8; i++)
				{
					if (targetNode.HasConnectionInDirection(i))
					{
						continue;
					}
					CustomGridNode customGridNode = (CustomGridNode)targetNode.GetNeighbourAlongDirection(i, checkConnectivity: false);
					if (customGridNode != null)
					{
						Vector3 vector = (customGridNode.ClosestPointOnNode(end).To2D().To3D() - end).To2D().To3D();
						if (vector.magnitude < movementCorpulence)
						{
							Vector3 v = -1f * (vector.normalized * movementCorpulence - vector);
							end += v.To2D().To3D();
						}
					}
				}
			}
			return end;
		}
	}

	public void UpdateVelocity()
	{
		if (IsTraverseInProgress)
		{
			m_Velocity = Vector3.zero;
		}
		else
		{
			m_Velocity = (IsReallyMoving ? m_NextVelocity : Vector3.zero);
		}
	}

	protected void ResetStuck()
	{
		m_StuckTimeStop = 0f;
		m_StuckTimeDirection = 0f;
		m_MinWaypointDistance = 1000000f;
	}

	protected void CompleteMovement(bool interrupted)
	{
		if (interrupted && m_Destination.HasValue && (bool)Unit)
		{
			Unit.OnMovementInterrupted(m_Destination.Value);
		}
		Stop();
		if ((bool)Unit)
		{
			Unit.OnMovementComplete();
		}
	}

	protected void UpdateAvoidance(ref Vector2 desiredDir, ref float desiredSpeed, out bool intersectsObstacles, out bool hasNavmeshObstacles)
	{
		intersectsObstacles = false;
		if (AvoidanceDisabled)
		{
			hasNavmeshObstacles = false;
			return;
		}
		Vector2 vector = Position.To2D();
		Vector2 nextWaypoint = m_NextWaypoint;
		float b = Vector2.Distance(vector, nextWaypoint);
		float num = Mathf.Min(3f, b);
		ObstacleAnalyzer obstacleAnalyzer = new ObstacleAnalyzer(Position, desiredDir, Corpulence, MaxSpeed);
		using (ProfileScope.New("Navmesh Avoidance", this))
		{
			hasNavmeshObstacles = obstacleAnalyzer.AddNavmeshObstacles();
		}
		using (ProfileScope.New("Units Avoidance", this))
		{
			BaseUnitEntity obj = m_Owner.GetComponent<UnitEntityView>()?.Data;
			bool flag = obj?.IsInPlayerParty ?? false;
			SortedSet<BaseUnitEntity> sortedSet = obj?.Vision.CanBeInRange;
			if (sortedSet != null)
			{
				foreach (BaseUnitEntity item in sortedSet)
				{
					if (!flag || !item.IsInPlayerParty)
					{
						UpdateUnitAvoidance(ref obstacleAnalyzer, desiredDir, ref intersectsObstacles, item, vector, num, nextWaypoint);
					}
				}
			}
			else
			{
				foreach (BaseUnitEntity item2 in EntityBoundsHelper.FindUnitsInRange(Position, num + Corpulence))
				{
					if (!flag || !item2.IsInPlayerParty)
					{
						UpdateUnitAvoidance(ref obstacleAnalyzer, desiredDir, ref intersectsObstacles, item2, vector, num, nextWaypoint);
					}
				}
			}
		}
		float num2 = 0f;
		using (ProfileScope.New("Calc Direction", this))
		{
			num2 = obstacleAnalyzer.CalcAvoidanceDirection(m_LastTurnAngle);
		}
		if ((double)Mathf.Abs(num2) > 10000.0)
		{
			m_LastTurnAngle = 0f;
			desiredSpeed = 0f;
			return;
		}
		m_LastTurnAngle = num2;
		if (!obstacleAnalyzer.MainDirectionBlockedByStatic && obstacleAnalyzer.ShouldSlowDown)
		{
			bool num3 = m_SlowDownTime > 0f;
			SlowDown(ref desiredSpeed);
			if (!num3)
			{
				return;
			}
		}
		desiredDir = Rotate(desiredDir, 0f - num2);
	}

	private static Vector2 Rotate(Vector2 v, float degrees)
	{
		float num = Mathf.Sin(degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(degrees * (MathF.PI / 180f));
		float x = v.x;
		float y = v.y;
		v.x = num2 * x - num * y;
		v.y = num * x + num2 * y;
		return v;
	}

	private void UpdateUnitAvoidance(ref ObstacleAnalyzer obstacleAnalyzer, Vector2 desiredDir, ref bool intersectsObstacles, BaseUnitEntity unit, Vector2 pA, float maxObstacleDistance, Vector2 dest)
	{
		if (unit == null)
		{
			return;
		}
		UnitMovementAgentBase movementAgent = unit.View.MovementAgent;
		if (!movementAgent || movementAgent.AvoidanceDisabled || movementAgent == this)
		{
			return;
		}
		Vector2 vector = movementAgent.Position.To2D();
		Vector2 velB = movementAgent.m_Velocity.To2D();
		float num = Corpulence + movementAgent.Corpulence;
		float num2 = Vector2.Distance(pA, vector);
		float num3 = maxObstacleDistance + num;
		if (!(num2 > num3))
		{
			if (num2 < num)
			{
				intersectsObstacles = true;
			}
			if (movementAgent.ChargingAvoidance == ChargingAvoidance && (!movementAgent.IsReallyMoving || !(num2 > num) || !(movementAgent.SpeedIndicator > 0.9f) || !(Vector2.Dot(movementAgent.MoveDirection, desiredDir) > s_IgnoreAngleCos)) && (movementAgent.IsReallyMoving || !(Vector2.Dot(vector - dest, pA - dest) < 0f) || !(num2 > num)))
			{
				float coreCorpulenceDelta = (IsSoftObstacle(movementAgent) ? 10f : 0.3f);
				obstacleAnalyzer.AddObstacle(vector, velB, num, coreCorpulenceDelta);
			}
		}
	}

	private void InitMoveDirection()
	{
		MoveDirection = ((Unit != null) ? Unit.EntityData.OrientationDirection.To2D() : base.transform.forward.To2D());
	}

	protected void SlowDown(ref float desiredSpeed)
	{
		if (!IsCharging)
		{
			if (m_SlowDownTime <= 0f)
			{
				m_SlowDownTime = 0.25f;
			}
			else
			{
				m_SlowDownTime = Mathf.Max(m_SlowDownTime, 0.1f);
			}
		}
	}

	private void ClaimPath([CanBeNull] Path path)
	{
		path?.Claim(this);
	}

	private void ReleasePath([CanBeNull] Path path)
	{
		path?.Release(this);
	}

	protected virtual void StartMovingWithPath([NotNull] ForcedPath path, bool forcedPath, bool requestedNewPath)
	{
		if (requestedNewPath)
		{
			m_LastTurnAngle = 0f;
			m_FirstTick = true;
		}
		if (!m_Destination.HasValue)
		{
			return;
		}
		if (path.vectorPath.Count > 1)
		{
			List<Vector3> vectorPath = path.vectorPath;
			Vector3 vector = vectorPath[vectorPath.Count - 1];
			List<Vector3> vectorPath2 = path.vectorPath;
			Vector3 normalized = (vector - vectorPath2[vectorPath2.Count - 2]).normalized;
			List<Vector3> vectorPath3 = path.vectorPath;
			Vector3 pos = vectorPath3[vectorPath3.Count - 1] - normalized * 0.0001f;
			List<Vector3> vectorPath4 = path.vectorPath;
			int index = vectorPath4.Count - 1;
			Vector3 value;
			if (!Game.Instance.CurrentlyLoadedArea.IsNavmeshArea || !(AstarPath.active != null))
			{
				List<Vector3> vectorPath5 = path.vectorPath;
				value = vectorPath5[vectorPath5.Count - 1];
			}
			else
			{
				value = ObstacleAnalyzer.FindClosestPointToStandOn(pos, Corpulence);
			}
			vectorPath4[index] = value;
		}
		m_IsInForceMode = forcedPath;
		Vector3 vector2;
		if (path.vectorPath.Count <= 0)
		{
			vector2 = Vector3.zero;
		}
		else
		{
			List<Vector3> vectorPath6 = path.vectorPath;
			vector2 = vectorPath6[vectorPath6.Count - 1];
		}
		Vector3 pathDestination = vector2;
		ObstaclePathingResult obstaclePathingResult = ((CombatMode && !forcedPath) ? ObstaclePathfinder.PathAroundStandingObstacles(path, this, null) : ObstaclePathingResult.PathClear);
		bool flag = !path.error && obstaclePathingResult != ObstaclePathingResult.NoPath;
		m_NoPathDestination = (flag ? null : m_Destination);
		if (!flag)
		{
			PartUnitCommands partUnitCommands = ((Unit != null && Unit.EntityData != null) ? Unit.EntityData.Commands : null);
			if (partUnitCommands != null && (partUnitCommands.HasAiCommand || obstaclePathingResult != ObstaclePathingResult.NoPath || path.vectorPath.Count <= 1))
			{
				if (Unit != null)
				{
					Unit.OnPathNotFound();
				}
				return;
			}
			List<Vector3> vectorPath7 = path.vectorPath;
			pathDestination = vectorPath7[vectorPath7.Count - 1];
		}
		if (path.vectorPath.Count == 1)
		{
			FinalDirection = Unit.Data.Forward;
			CompleteMovement(interrupted: false);
			return;
		}
		Path = path;
		CalculateOffsetForLastPathNode();
		SetWaypoint(1);
		if (Unit != null)
		{
			Unit.OnMovementStarted(pathDestination);
		}
		m_NodeLinkTraverser.Reset();
		ObstaclesHelper.RemoveFromGroup(this);
		List<Vector3> vectorPath8 = path.vectorPath;
		Vector3 vector3 = vectorPath8[vectorPath8.Count - 1];
		List<Vector3> vectorPath9 = path.vectorPath;
		FinalDirection = (vector3 - vectorPath9[vectorPath9.Count - 2]).normalized;
	}

	protected void SetWaypoint(int index)
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
		if (Path.vectorPath.TryGet(index, out var element) && Game.Instance.CurrentlyLoadedArea.IsNavmeshArea && AstarPath.active != null)
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
		for (int i = index; i < Path.vectorPath.Count - 1; i++)
		{
			Vector2 a = Path.vectorPath[i].To2D();
			Vector2 b = Path.vectorPath[i + 1].To2D();
			m_RemainingPathDistance += Vector2.Distance(a, b);
		}
		if (m_Destination.HasValue)
		{
			List<Vector3> vectorPath = Path.vectorPath;
			Vector3 v = vectorPath[vectorPath.Count - 1];
			m_RemainingPathDistance += Vector2.Distance(v.To2D(), m_Destination.Value.To2D());
		}
		m_RemainingPathDistance -= ApproachRadius;
		if (Unit != null)
		{
			Unit.OnMovementWaypointUpdate(index);
		}
		ResetStuck();
	}

	protected virtual Vector2 GetNextWaypoint2D(int index)
	{
		return Path.vectorPath[index].To2D();
	}

	protected virtual Vector3 GetNextWaypoint3D(int index)
	{
		return Path.vectorPath[index];
	}

	private bool IsSoftObstacle(UnitMovementAgentBase obstacle)
	{
		if (!WantsToMove)
		{
			return false;
		}
		return ((Unit != null && Unit.EntityData != null) ? Unit.EntityData.GetFactionOptional() : null) != ((obstacle.Unit != null && obstacle.Unit.EntityData != null) ? obstacle.Unit.EntityData.GetFactionOptional() : null);
	}

	public void Stop()
	{
		IsStopping = false;
		Path = null;
		m_Speed = 0f;
		m_Destination = null;
		m_NextPointIndex = 0;
		m_Velocity = Vector3.zero;
		m_NodeLinkTraverser.Reset();
		ObstaclesHelper.ConnectToGroups(this);
	}

	private void OnDrawGizmos()
	{
		if (UnitContacts == null)
		{
			return;
		}
		foreach (UnitMovementAgentBase unitContact in UnitContacts)
		{
			if ((bool)unitContact)
			{
				Debug.DrawLine(Position + s_DebugShift, unitContact.transform.position + s_DebugShift, Color.magenta);
			}
		}
	}

	public bool IsValid()
	{
		if (Unit != null && Unit.EntityData != null)
		{
			return Unit.Blueprint != null;
		}
		return false;
	}

	private void CalculateOffsetForLastPathNode()
	{
		if (AstarPath.active == null || (Path?.vectorPath?.Count).GetValueOrDefault() == 0)
		{
			return;
		}
		List<Vector3> vectorPath = Path.vectorPath;
		CustomGridNode customGridNode = ObstacleAnalyzer.GetNearestNode(vectorPath[vectorPath.Count - 1]).node as CustomGridNode;
		CustomGridGraph customGridGraph = customGridNode?.Graph as CustomGridGraph;
		if (customGridNode == null || customGridGraph == null)
		{
			PFLog.Default.Warning("Can't calculate neighbours walkability for offset");
			return;
		}
		float num = customGridGraph.nodeSize / 2f;
		Vector3 vector = (Vector3)customGridNode.position;
		Vector3[][] neighboursRectOffset = m_NeighboursRectOffset;
		neighboursRectOffset[0][0] = new Vector3(0f - num, 0f, 0f - num);
		neighboursRectOffset[0][1] = new Vector3(num, 0f, 0f - num + m_OffsetForLastNode);
		neighboursRectOffset[1][0] = new Vector3(num - m_OffsetForLastNode, 0f, 0f - num);
		neighboursRectOffset[1][1] = new Vector3(num, 0f, num);
		neighboursRectOffset[2][0] = new Vector3(0f - num, 0f, num - m_OffsetForLastNode);
		neighboursRectOffset[2][1] = new Vector3(num, 0f, num);
		neighboursRectOffset[3][0] = new Vector3(0f - num, 0f, 0f - num);
		neighboursRectOffset[3][1] = new Vector3(0f - num + m_OffsetForLastNode, 0f, num);
		neighboursRectOffset[4][0] = new Vector3(num - m_OffsetForLastNode, 0f, 0f - num);
		neighboursRectOffset[4][1] = new Vector3(num, 0f, 0f - num + m_OffsetForLastNode);
		neighboursRectOffset[5][0] = new Vector3(num - m_OffsetForLastNode, 0f, num - m_OffsetForLastNode);
		neighboursRectOffset[5][1] = new Vector3(num, 0f, num);
		neighboursRectOffset[6][0] = new Vector3(0f - num, 0f, num - m_OffsetForLastNode);
		neighboursRectOffset[6][1] = new Vector3(0f - num + m_OffsetForLastNode, 0f, num);
		neighboursRectOffset[7][0] = new Vector3(0f - num, 0f, 0f - num);
		neighboursRectOffset[7][1] = new Vector3(0f - num + m_OffsetForLastNode, 0f, 0f - num + m_OffsetForLastNode);
		Vector3[] offsetDirection = m_OffsetDirection;
		offsetDirection[0] = new Vector3(0f, 0f, 0f - num + m_OffsetForLastNode);
		offsetDirection[1] = new Vector3(num - m_OffsetForLastNode, 0f, 0f);
		offsetDirection[2] = new Vector3(0f, 0f, num - m_OffsetForLastNode);
		offsetDirection[3] = new Vector3(0f - num + m_OffsetForLastNode, 0f, 0f);
		offsetDirection[4] = new Vector3(num - m_OffsetForLastNode, 0f, 0f - num + m_OffsetForLastNode);
		offsetDirection[5] = new Vector3(num - m_OffsetForLastNode, 0f, num - m_OffsetForLastNode);
		offsetDirection[6] = new Vector3(0f - num + m_OffsetForLastNode, num - m_OffsetForLastNode);
		offsetDirection[7] = new Vector3(0f - num + m_OffsetForLastNode, 0f, 0f - num + m_OffsetForLastNode);
		List<Vector3> vectorPath2 = Path.vectorPath;
		Vector3 vector2 = vectorPath2[vectorPath2.Count - 1] - vector;
		int num2 = 8;
		bool[] array = new bool[num2];
		for (int i = 0; i < num2; i++)
		{
			CustomGridNode customGridNode2 = customGridGraph.nodes[customGridNode.NodeInGridIndex + customGridGraph.neighbourOffsets[i]];
			array[i] = customGridNode2.Walkable;
		}
		for (int j = 0; j < num2; j++)
		{
			if (!array[j] && vector2.x >= neighboursRectOffset[j][0].x - 0.0001f && vector2.x <= neighboursRectOffset[j][1].x + 0.0001f && vector2.z >= neighboursRectOffset[j][0].z - 0.0001f && vector2.z <= neighboursRectOffset[j][1].z + 0.0001f)
			{
				if (offsetDirection[j].x != 0f)
				{
					vector2.x = offsetDirection[j].x;
				}
				if (offsetDirection[j].z != 0f)
				{
					vector2.z = offsetDirection[j].z;
				}
			}
		}
		List<Vector3> vectorPath3 = Path.vectorPath;
		vectorPath3[vectorPath3.Count - 1] = vector2 + vector;
	}

	public void HandleUnitLifeStateChanged(UnitLifeState prevLifeState)
	{
		UpdateBlocker();
	}

	public void UpdateBlocker()
	{
		if (Blocker == null)
		{
			return;
		}
		bool flag = IsNodeBlockNeeded();
		if (Blocker.IsBlocking != flag)
		{
			if (flag)
			{
				Blocker.BlockAtCurrentPosition();
			}
			else
			{
				Blocker.Unblock();
			}
		}
	}

	public void ReinitBlocker(UnitEntity entity)
	{
		m_Blocker = new WarhammerSingleNodeBlocker(entity);
		m_TraversalProvider = CreateTraversalProvider(Unit, m_Blocker);
		UpdateBlocker();
	}

	private bool IsNodeBlockNeeded()
	{
		if (Unit != null && !Unit.Data.LifeState.IsDeadOrUnconscious)
		{
			return !Unit.Data.HasMechanicFeature(MechanicsFeatureType.Hidden);
		}
		return false;
	}

	public static float GetDistanceToSegment(Vector3 start, Vector3 end, Vector3 point)
	{
		Vector3 rhs = end - start;
		float num = 0f - (rhs.x * start.x + rhs.y * start.y + rhs.z * start.z);
		float num2 = 0f - (rhs.x * end.x + rhs.y * end.y + rhs.z * end.z);
		bool num3 = rhs.x * point.x + rhs.y * point.y + rhs.z * point.z + num < 0f;
		bool flag = rhs.x * point.x + rhs.y * point.y + rhs.z * point.z + num2 < 0f;
		if (num3)
		{
			return Vector3.Distance(start, point);
		}
		if (!flag)
		{
			return Vector3.Distance(end, point);
		}
		return Vector3.Cross(start - point, rhs).magnitude / rhs.magnitude;
	}
}
