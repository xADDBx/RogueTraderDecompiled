using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.FogOfWar.LineOfSight;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Covers;
using Kingmaker.View.Equipment;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.Blueprints;
using Kingmaker.Visual.Trails;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Projectiles;

public class Projectile : IInterpolatable
{
	private const int LayersMaskForHit = 134742273;

	private const float UpShiftDistance = 5f;

	private static readonly Vector3 UpShift = Vector3.up * 5f;

	private Vector3 m_LaunchPosition;

	private Vector3 m_TargetPosition;

	private float m_Progress;

	private UnitViewHandSlotData m_ShieldToHit;

	private Vector3 m_LocalShieldHitPos;

	private bool m_FirstTick = true;

	[CanBeNull]
	private RulePerformAttack m_AttackWithWeapon;

	private int m_LaunchFrame;

	private Vector3? m_HullHitPos;

	private bool m_CachedVoidShieldHitPos;

	private float m_Duration;

	private float? m_MaxDuration;

	private Vector3 m_MisdirectionOffset;

	private Transform m_TargetLocator;

	private RaycastHit[] m_Hits;

	private Vector3 m_PrevCorePosition;

	private float m_PrevPassedDistance;

	private float m_PrevPassedTime;

	private float m_PrevProgress;

	[NotNull]
	public BlueprintProjectile Blueprint { get; }

	[NotNull]
	public TargetWrapper Launcher { get; }

	[NotNull]
	public TargetWrapper Target { get; private set; }

	public GameObject View { get; private set; }

	public float PassedDistance { get; private set; }

	public float PassedTime { get; private set; }

	public float TimeAfterHit { get; private set; }

	public float LaunchHeight { get; private set; }

	public bool IsHit { get; private set; }

	public bool IsHitWall { get; private set; }

	public bool IsHitHandled { get; private set; }

	public Vector3 CorePosition { get; private set; }

	public bool MissHitNothing { get; private set; }

	public bool GoingToHitVoidShield { get; private set; } = true;


	public Vector3? VoidShieldHitPos { get; private set; }

	public float Speed { get; private set; }

	public float DeterministicDistance { get; private set; }

	[CanBeNull]
	public AbilityData Ability { get; set; }

	public Action<Projectile> OnHitCallback { get; set; }

	public RulebookEvent OnHitRule { get; set; }

	public RulebookEventContext SavedContext { get; set; }

	public AttackResult AttackResult { get; set; } = AttackResult.Hit;


	public StarshipHitLocation StarshipHitLocation { get; set; }

	public float MaxRangeMeters { get; set; }

	public bool Cleared { get; set; }

	public bool IsFromWeapon { get; set; }

	public int Index { get; set; }

	public bool IsCoverHit { get; set; }

	public bool InvertUpDirectionForTrajectories { get; set; }

	public bool FollowLauncher { get; set; }

	public bool DoNotPlayHitEffect { get; set; }

	public bool DoNotDeliverHit { get; set; }

	public float TimeSinceLaunch => PassedTime + TimeAfterHit;

	public RaycastHit[] Hits => m_Hits;

	public RaycastHit ClosestHit => Hits.MinBy((RaycastHit hit) => Vector3.SqrMagnitude(hit.point - LaunchPosition));

	[CanBeNull]
	public BaseUnitEntity TargetUnitForHitSnapFx { get; set; }

	public Vector3 LaunchPosition
	{
		get
		{
			if (!FollowLauncher || Blueprint.FreezeLaunchPosition)
			{
				return m_LaunchPosition;
			}
			return Launcher.Point;
		}
		set
		{
			CorePosition = (m_PrevCorePosition = (m_LaunchPosition = value));
		}
	}

	[CanBeNull]
	public RulePerformAttack AttackWithWeapon
	{
		get
		{
			return m_AttackWithWeapon;
		}
		set
		{
			m_AttackWithWeapon = value;
			AttackResult = value?.Result ?? AttackResult.Hit;
		}
	}

	public bool Destroyed
	{
		get
		{
			if (IsHit)
			{
				return TimeAfterHit >= Blueprint.LifetimeParticlesAfterHit;
			}
			return false;
		}
	}

	public bool IsAlive
	{
		get
		{
			if (!Destroyed)
			{
				return !Cleared;
			}
			return false;
		}
	}

	public Projectile([NotNull] BlueprintProjectile blueprint, [NotNull] TargetWrapper launcher, [NotNull] TargetWrapper target)
	{
		Blueprint = blueprint;
		Launcher = launcher;
		Target = target;
	}

	public void AttachView(GameObject view)
	{
		if (View != null)
		{
			PFLog.Default.ErrorWithReport("Projectile view already attached");
		}
		View = view.gameObject;
		if (Blueprint.Trajectory == null)
		{
			return;
		}
		BlueprintProjectileTrajectory trajectory = Blueprint.Trajectory;
		if (!trajectory.PlaneOffset.Empty())
		{
			TrajectoryOffset[] planeOffset = trajectory.PlaneOffset;
			foreach (TrajectoryOffset trajectoryOffset in planeOffset)
			{
				trajectoryOffset.OnInitializeStaticOffset = PFStatefulRandom.Visuals.Fx.Range(0f - trajectoryOffset.RandomOffset, trajectoryOffset.RandomOffset);
			}
		}
		if (!trajectory.UpOffset.Empty())
		{
			TrajectoryOffset[] planeOffset = trajectory.UpOffset;
			foreach (TrajectoryOffset trajectoryOffset2 in planeOffset)
			{
				trajectoryOffset2.OnInitializeStaticOffset = PFStatefulRandom.Visuals.Fx.Range(0f - trajectoryOffset2.RandomOffset, trajectoryOffset2.RandomOffset);
			}
		}
	}

	public Vector3 GetTargetPoint()
	{
		if (Target.Entity is StarshipEntity starshipTarget)
		{
			return GetTargetPointForStarship(starshipTarget);
		}
		if (Blueprint.FreezeEndPosition && m_TargetPosition != Vector3.zero)
		{
			return m_TargetPosition;
		}
		if (m_TargetLocator != null)
		{
			return m_TargetLocator.position + m_MisdirectionOffset;
		}
		return Target.Point + m_MisdirectionOffset;
	}

	private Vector3 GetTargetPointForStarship([NotNull] StarshipEntity starshipTarget)
	{
		StarshipView componentInChildren = starshipTarget.View.GetComponentInChildren<StarshipView>();
		if (componentInChildren == null)
		{
			return starshipTarget.Position + Vector3.up;
		}
		List<Vector3> hitPointsOnHull = GetHitPointsOnHull(componentInChildren);
		if (!m_HullHitPos.HasValue)
		{
			Vector3 vector = hitPointsOnHull[PFStatefulRandom.Controllers.Projectiles.Range(0, hitPointsOnHull.Count - 1)];
			Vector3 vector2 = componentInChildren.transform.localToWorldMatrix * vector;
			m_HullHitPos = componentInChildren.transform.position + vector2;
		}
		int currentShields = starshipTarget.Shields.GetCurrentShields(StarshipHitLocation);
		if (currentShields > 0 && !m_CachedVoidShieldHitPos)
		{
			if (View == null)
			{
				VoidShieldHitPos = GetVoidShieldHitPos(m_HullHitPos.Value, Launcher.Point, starshipTarget.View.gameObject);
			}
			else
			{
				VoidShieldHitPos = GetVoidShieldHitPos(m_HullHitPos.Value, View.transform.position, starshipTarget.View.gameObject);
				m_CachedVoidShieldHitPos = true;
			}
		}
		if (currentShields > 0)
		{
			return VoidShieldHitPos ?? Target.Point;
		}
		return m_HullHitPos ?? Target.Point;
	}

	public void BeforeLaunch()
	{
		m_LaunchFrame = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		if (Blueprint.FollowTerrain && Physics.Raycast(LaunchPosition + UpShift, Vector3.down, out var hitInfo, 10f, 2359553))
		{
			LaunchHeight = hitInfo.distance - 5f;
		}
		m_HullHitPos = null;
		VoidShieldHitPos = null;
		m_CachedVoidShieldHitPos = false;
		GoingToHitVoidShield = true;
		DeterministicDistance = this.Distance(Launcher.Point, Target.Point);
		Speed = Blueprint.Speed + PFStatefulRandom.Controllers.Projectiles.Range(0f - Blueprint.SpeedDelta, Blueprint.SpeedDelta);
		Speed *= Game.CombatAnimSpeedUp;
		m_Duration = DeterministicDistance / Speed;
		if (Blueprint.HasMinTime && m_Duration < Blueprint.MinTime)
		{
			Speed = DeterministicDistance / Blueprint.MinTime;
			m_Duration = Blueprint.MinTime;
		}
		m_MaxDuration = ((0f < MaxRangeMeters) ? new float?(MaxRangeMeters / Speed) : null);
		if (Target.Entity is UnitEntity unitEntity)
		{
			BlueprintFxLocatorGroup group = Ability?.FXSettings?.VisualFXSettings?.ProjectileTarget ?? FxRoot.Instance.LocatorGroupTorso;
			m_TargetLocator = ObjectExtensions.Or(unitEntity.View.ParticlesSnapMap, null)?.GetLocators(group).Random(PFStatefulRandom.Controllers.Projectiles)?.Transform;
			if (m_TargetLocator != null)
			{
				m_TargetPosition = m_TargetLocator.position + m_MisdirectionOffset;
			}
		}
		else
		{
			m_TargetPosition = Target.Point + m_MisdirectionOffset;
		}
		Vector3 launchPosition = LaunchPosition;
		Vector3 direction = GetTargetPoint() - launchPosition;
		float maxDistance = direction.magnitude + 0.1f;
		m_Hits = Physics.RaycastAll(launchPosition, direction, maxDistance, 134742273);
	}

	public void Tick()
	{
		if (m_LaunchFrame == Game.Instance.RealTimeController.CurrentSystemStepIndex)
		{
			return;
		}
		if (m_FirstTick)
		{
			m_FirstTick = false;
		}
		float deltaTime = Game.Instance.TimeController.DeltaTime;
		if (IsHit)
		{
			TimeAfterHit += deltaTime;
			return;
		}
		Vector3 corePosition = CorePosition;
		Vector3 targetPoint = GetTargetPoint();
		float num = deltaTime * Speed;
		if (Blueprint.FollowTerrain)
		{
			targetPoint.y = corePosition.y;
		}
		m_PrevCorePosition = CorePosition;
		CorePosition = Vector3.MoveTowards(corePosition, targetPoint, num);
		if (Blueprint.FollowTerrain)
		{
			CustomGridNodeBase targetNode;
			Vector3 corePosition2 = UnitMovementAgentBase.Move(corePosition, CorePosition - corePosition, 0.3f, out targetNode);
			corePosition2.y += LaunchHeight;
			CorePosition = corePosition2;
			Vector3 from = corePosition + LosCalculations.EyeShift;
			if (LineOfSightGeometry.Instance.HasObstacle(from, CorePosition))
			{
				IsHit = true;
				IsHitWall = true;
			}
		}
		m_PrevPassedTime = PassedTime;
		m_PrevPassedDistance = PassedDistance;
		PassedTime += deltaTime;
		PassedDistance += num;
		if (m_Duration <= PassedTime || m_MaxDuration <= PassedTime)
		{
			if (MaxRangeMeters > 0f)
			{
				m_PrevPassedDistance = PassedDistance;
				PassedDistance = MaxRangeMeters;
			}
			if (Target?.Entity is StarshipEntity starshipEntity)
			{
				if (GoingToHitVoidShield)
				{
					if (starshipEntity.Shields.GetCurrentShields(StarshipHitLocation) > 0)
					{
						IsHit = true;
					}
					else
					{
						GoingToHitVoidShield = false;
					}
				}
				else
				{
					IsHit = true;
				}
			}
			else
			{
				GoingToHitVoidShield = false;
				IsHit = true;
			}
		}
		m_PrevProgress = m_Progress;
		m_Progress = ((m_Duration > 0f) ? (PassedTime / m_Duration) : 1f);
		UpdateViewPosition(IsHit ? 1f : 0f);
	}

	void IInterpolatable.Tick(float interpolationProgress)
	{
		if (!m_FirstTick && !IsHit && !Destroyed && !Cleared)
		{
			UpdateViewPosition(interpolationProgress);
		}
	}

	private void UpdateViewPosition(float interpolationProgress)
	{
		Vector3 vector = Vector3.LerpUnclamped(m_PrevCorePosition, CorePosition, interpolationProgress);
		float passedDistance = Mathf.LerpUnclamped(m_PrevPassedDistance, PassedDistance, interpolationProgress);
		float time = Mathf.LerpUnclamped(m_PrevPassedTime, PassedTime, interpolationProgress);
		float progress = Mathf.LerpUnclamped(m_PrevProgress, m_Progress, interpolationProgress);
		Vector3 targetPoint = GetTargetPoint();
		float fullDistance = this.Distance(targetPoint, LaunchPosition);
		Vector3 vector2 = ((Blueprint.Trajectory != null) ? TrajectoryCalculator.CalculateShift(Blueprint.Trajectory, targetPoint - LaunchPosition, fullDistance, passedDistance, progress, time, InvertUpDirectionForTrajectories) : Vector3.zero);
		Vector3 vector3 = vector + vector2;
		if (!IsHit)
		{
			View.transform.LookAt(vector3);
		}
		View.transform.position = vector3;
	}

	private Vector3 GetVoidShieldHitPos(Vector3 hullHitPosition, Vector3 launchPos, GameObject target)
	{
		RaycastHit[] array = Physics.RaycastAll(launchPos, hullHitPosition - launchPos, 100f, 16777216);
		if (array != null && array.Length != 0)
		{
			RaycastHit[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				RaycastHit raycastHit = array2[i];
				FxLocator component = raycastHit.transform.parent.GetComponent<FxLocator>();
				if (!(null == component) && component.particleMap.gameObject == target)
				{
					return raycastHit.point;
				}
			}
		}
		return hullHitPosition;
	}

	public void HandleHit()
	{
		IsHitHandled = true;
		FxHelper.Stop(View);
		CompositeTrailRenderer[] componentsInChildren = View.GetComponentsInChildren<CompositeTrailRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			foreach (TrailEmitter emitter in componentsInChildren[i].Emitters)
			{
				if (emitter.DontDestroyOnDisable)
				{
					if (emitter.Spawner != null)
					{
						emitter.Spawner.SetActive(value: false);
					}
					if (emitter.SecondSpawner != null)
					{
						emitter.SecondSpawner.SetActive(value: false);
					}
				}
			}
		}
		EventBus.RaiseEvent(delegate(IProjectileHitHandler h)
		{
			h.HandleProjectileHit(this);
		});
		OnHitCallback?.Invoke(this);
		if (OnHitRule != null)
		{
			if (SavedContext != null)
			{
				SavedContext.Trigger(OnHitRule);
			}
			else
			{
				Rulebook.Trigger(OnHitRule);
			}
		}
	}

	private List<Vector3> GetHitPointsOnHull(StarshipView starShipViewTarget)
	{
		if (Launcher.Entity == null)
		{
			throw new Exception("Launcher entity is missing");
		}
		if (Target.Entity == null)
		{
			throw new Exception("Target entity is missing");
		}
		StarshipFxHitMask starshipFxHitMask = starShipViewTarget.starshipFxHitMask;
		Vector3 forward = Target.Entity.View.ViewTransform.forward;
		Vector3 normalized = (Launcher.Entity.Position - Target.Entity.Position).normalized;
		float num = Vector2.Dot(new Vector2(normalized.x, normalized.z).normalized, new Vector2(forward.x, forward.z).normalized);
		List<Vector3> result = new List<Vector3>();
		if (num >= 0.5f)
		{
			result = starshipFxHitMask.frontHitPositions;
		}
		if (num <= -0.5f)
		{
			result = starshipFxHitMask.backHitPositions;
		}
		if (num > -0.5f && num < 0.5f)
		{
			result = ((Vector3.Dot(forward, Vector3.forward) < 0f) ? ((!(Launcher.Entity.Position.x > Target.Entity.Position.x)) ? starshipFxHitMask.rightHitPositions : starshipFxHitMask.leftHitPositions) : ((!(Launcher.Entity.Position.x > Target.Entity.Position.x)) ? starshipFxHitMask.leftHitPositions : starshipFxHitMask.rightHitPositions));
		}
		return result;
	}

	public void SetMisdirectionOffset(Vector3 offset)
	{
		m_MisdirectionOffset = offset;
	}

	internal void OnLaunch()
	{
		Game.Instance.InterpolationController.Add(this);
	}

	internal void OnRelease()
	{
		Game.Instance.InterpolationController.Remove(this);
	}
}
