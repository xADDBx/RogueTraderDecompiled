using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ResourceLinks;
using Kingmaker.Visual.HitSystem;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints;

[TypeId("6370c9530f88b5c41bd101d35b8e4fd2")]
public class BlueprintProjectile : BlueprintScriptableObject, IResourcesHolder
{
	public float Speed = 50f;

	public float SpeedDelta;

	public float MinTime;

	public ProjectileLink View;

	public PrefabLink CastFx;

	public float CastFxLifetime = 1f;

	public float LifetimeParticlesAfterHit = 1f;

	public ProjectileHitSettings ProjectileHit;

	public DamageHitSettings DamageHit;

	public bool UseSourceBoneScale;

	public float AddRagdollImpulse;

	[SerializeField]
	[FormerlySerializedAs("Trajectory")]
	private BlueprintProjectileTrajectoryReference m_Trajectory;

	public bool FollowTerrain;

	public bool FreezeLaunchPosition;

	public bool FreezeEndPosition;

	[InfoBox("If set, projectile will emits from launcher entity, not from grid node")]
	public bool IgnoreGrid;

	public bool HasMinTime => 0f < MinTime;

	public BlueprintProjectileTrajectory Trajectory => m_Trajectory?.Get();
}
