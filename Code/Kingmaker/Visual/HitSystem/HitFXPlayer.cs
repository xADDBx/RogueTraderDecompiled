using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.HitSystem;

public static class HitFXPlayer
{
	[NotNull]
	private static readonly List<GameObject> Hits = new List<GameObject>();

	[NotNull]
	private static readonly List<GameObject> TopLayerHits = new List<GameObject>();

	[NotNull]
	private static readonly List<GameObject> UnitHits = new List<GameObject>();

	[NotNull]
	private static readonly List<HitCollection> UsedHits = new List<HitCollection>();

	private static void Init()
	{
		Hits.Clear();
		TopLayerHits.Clear();
		UnitHits.Clear();
		UsedHits.Clear();
	}

	public static Vector3 GetProjectileHitFxSpawnPosition([NotNull] Projectile projectile, [NotNull] TargetWrapper target)
	{
		MechanicEntityView target2 = target.Entity?.View;
		return GetHitFxSpawnPosition(projectile, target2, null, adjustHeight: false);
	}

	public static void PlayProjectileHit([NotNull] Projectile projectile, [NotNull] TargetWrapper target)
	{
		if (projectile.DoNotPlayHitEffect)
		{
			return;
		}
		if (projectile.Target.Entity is StarshipEntity && projectile.GoingToHitVoidShield)
		{
			SpawnVoidShieldHitFx(projectile);
			return;
		}
		Init();
		ProjectileHitSettings projectileHit = projectile.Blueprint.ProjectileHit;
		switch (projectile.AttackResult)
		{
		case AttackResult.Hit:
		case AttackResult.RighteousFury:
			AddHit(projectileHit.HitFx.Load());
			AddUnitHit(projectileHit.HitSnapFx.Load());
			break;
		case AttackResult.Miss:
		case AttackResult.Dodge:
			if (!projectile.MissHitNothing)
			{
				AddHit(projectileHit.MissFx.Load());
				AddHit(projectileHit.MissDecalFx.Load());
			}
			break;
		}
		MechanicEntityView mechanicEntityView = target.Entity?.View;
		BaseUnitEntity targetUnitForHitSnapFx = projectile.TargetUnitForHitSnapFx;
		MechanicEntityView snapFxTarget = ((targetUnitForHitSnapFx != null) ? ObjectExtensions.Or(targetUnitForHitSnapFx.View, null) : null) ?? mechanicEntityView;
		Vector3 hitFxSpawnPosition = GetHitFxSpawnPosition(projectile, mechanicEntityView, null, adjustHeight: false);
		Quaternion hitFxSpawnRotation = GetHitFxSpawnRotation(projectile, mechanicEntityView, null);
		PlayHits(projectile, mechanicEntityView, snapFxTarget, projectileHit.FollowTarget, in hitFxSpawnPosition, in hitFxSpawnRotation);
	}

	public static void PlayDamageHit([NotNull] RuleDealDamage damageRule)
	{
		if (!damageRule.ConcreteTarget.IsInFogOfWar && !damageRule.DisableFxAndSound)
		{
			PlayDamageHit(damageRule.ConcreteInitiator, damageRule.ConcreteTarget, damageRule.Reason.Context?.MainTarget, damageRule.Projectile, damageRule.Reason.Ability, damageRule.ResultValue, damageRule.ResultIsCritical, damageRule.IsDot);
		}
	}

	public static void PlayDamageHit(MechanicEntity initiator, MechanicEntity target, [CanBeNull] TargetWrapper mainTarget, [CanBeNull] Projectile projectile, AbilityData spell, DamageValue damage, bool isCritical, bool isDot)
	{
		HitSystemRoot hitSystemRoot = BlueprintRoot.Instance.HitSystemRoot;
		Vector3 zero = Vector3.zero;
		DamageHitSettings damageHitSettings;
		if (projectile != null)
		{
			damageHitSettings = projectile.Blueprint.DamageHit;
			zero = projectile.View.transform.forward;
			if (zero.y < 0f)
			{
				DamageData source = damage.Source;
				if (source != null && source.Type == DamageType.Impact)
				{
					zero = (target.Position - projectile.View.transform.position).normalized * projectile.Blueprint.AddRagdollImpulse;
					zero.y = 0.15f * projectile.Blueprint.AddRagdollImpulse;
				}
			}
		}
		else
		{
			damageHitSettings = hitSystemRoot.DefaultDamage;
			zero = (target.Position - initiator.Position).normalized;
		}
		MechanicEntityView mechanicEntityView = target?.View;
		UnitEntityView unitEntityView = mechanicEntityView as UnitEntityView;
		MechanicEntityView mechanicEntityView2 = initiator?.View;
		if (!mechanicEntityView || !mechanicEntityView2)
		{
			return;
		}
		Init();
		bool flag = false;
		HitLevel level = HitLevel.Minor;
		bool flag2 = false;
		bool flag3 = false;
		HitLevel hitLevel = ((isDot || flag3) ? HitLevel.Minor : damageHitSettings.HitLevel);
		hitLevel = CalcHitLevel(damage, hitLevel, isCritical);
		HitCollection damage2 = hitSystemRoot.GetDamage(damage.Source);
		GameObject gameObject = null;
		if (damage2 != null && !UsedHits.Contains(damage2))
		{
			UsedHits.Add(damage2);
			bool topLayer = true;
			gameObject = damage2.Select(hitLevel);
			AddHit(gameObject, topLayer);
			AddUnitHit(damage2.SelectSnap(hitLevel));
		}
		if (damage.FinalValue > 0 && !damage.Source.Immune && !flag)
		{
			flag = true;
			level = hitLevel;
			if (gameObject == null)
			{
				flag2 = true;
			}
		}
		if (flag && unitEntityView != null)
		{
			SurfaceType surfaceType = unitEntityView.Data.SurfaceType;
			if (damageHitSettings.BillboardBlood)
			{
				AddHit(hitSystemRoot.GetBillboardBlood(surfaceType, level, flag2), flag2);
			}
			if (damageHitSettings.DirectionalBlood)
			{
				AddHit(hitSystemRoot.GetDirectionalBlood(surfaceType, level), flag2);
			}
		}
		Vector3 hitFxSpawnPosition = GetHitFxSpawnPosition(projectile, mechanicEntityView, mechanicEntityView2, adjustHeight: false);
		Quaternion hitFxSpawnRotation = GetHitFxSpawnRotation(projectile, mechanicEntityView, mechanicEntityView2);
		PlayHits(projectile, mechanicEntityView, mechanicEntityView, damageHitSettings.FollowTarget, in hitFxSpawnPosition, in hitFxSpawnRotation);
		if (!(unitEntityView != null) || !(zero != Vector3.zero))
		{
			return;
		}
		DamageType type = damage.Source.Type;
		float addMagnitude = projectile?.Blueprint.AddRagdollImpulse ?? 0f;
		if (projectile == null)
		{
			ReferenceArrayProxy<BlueprintProjectile>? referenceArrayProxy = spell?.FXSettings?.VisualFXSettings?.Projectiles;
			if (referenceArrayProxy.HasValue && referenceArrayProxy.GetValueOrDefault().Length > 0 && (bool)referenceArrayProxy.Value[0])
			{
				addMagnitude = spell.FXSettings.VisualFXSettings.Projectiles[0].AddRagdollImpulse;
			}
		}
		unitEntityView.AddRagdollImpulse(zero, addMagnitude, type);
	}

	private static HitLevel CalcHitLevel(DamageValue damage, HitLevel hitLevel, bool crit)
	{
		if (hitLevel == HitLevel.Crit)
		{
			return HitLevel.Crit;
		}
		if (crit)
		{
			hitLevel++;
		}
		if (damage.Reduction > 0 || damage.Source.Immune)
		{
			hitLevel--;
		}
		return ClampHitLevel(hitLevel);
	}

	private static HitLevel ClampHitLevel(HitLevel hitLevel)
	{
		if (hitLevel < HitLevel.Minor)
		{
			return HitLevel.Minor;
		}
		if (hitLevel > HitLevel.Crit)
		{
			return HitLevel.Crit;
		}
		return hitLevel;
	}

	private static void AddHit([CanBeNull] GameObject hit, bool topLayer = false)
	{
		if (!(hit == null))
		{
			if (topLayer)
			{
				TopLayerHits.Add(hit);
			}
			else
			{
				Hits.Add(hit);
			}
		}
	}

	private static void AddUnitHit([CanBeNull] GameObject hit)
	{
		if (!(hit == null))
		{
			UnitHits.Add(hit);
		}
	}

	private static void PlayHits([CanBeNull] Projectile projectile, [CanBeNull] MechanicEntityView target, [CanBeNull] MechanicEntityView snapFxTarget, bool attach, in Vector3 hitFxSpawnPosition, in Quaternion hitFxSpawnRotation)
	{
		if ((ObjectExtensions.Or(target, null)?.EntityData?.IsInFogOfWar).GetValueOrDefault())
		{
			return;
		}
		try
		{
			if (projectile == null && target == null)
			{
				return;
			}
			MechanicEntityView attachEntity = (attach ? target : null);
			for (int i = 0; i < Hits.Count; i++)
			{
				GameObject hit = Hits[i];
				bool num = projectile?.Ability?.FXSettings?.SoundFXSettings != null;
				GameObject gameObject = PlayHit(hit, in hitFxSpawnPosition, in hitFxSpawnRotation, attachEntity);
				if (num && gameObject != null && gameObject.TryGetComponent<SoundFx>(out var component))
				{
					component.BlockSoundFXPlaying = true;
				}
			}
			for (int j = 0; j < TopLayerHits.Count; j++)
			{
				GameObject gameObject2 = PlayHit(TopLayerHits[j], in hitFxSpawnPosition, in hitFxSpawnRotation, attachEntity);
				if (gameObject2 != null)
				{
					PooledFx componentNonAlloc = gameObject2.GetComponentNonAlloc<PooledFx>();
					if ((bool)componentNonAlloc)
					{
						componentNonAlloc.AdjustLayerOrder(1);
					}
				}
			}
			if (snapFxTarget != null)
			{
				for (int k = 0; k < UnitHits.Count; k++)
				{
					FxHelper.SpawnFxOnEntity(UnitHits[k], snapFxTarget);
				}
			}
		}
		finally
		{
		}
	}

	private static Vector3 GetHitFxSpawnPosition([CanBeNull] Projectile projectile, [CanBeNull] MechanicEntityView target, [CanBeNull] MechanicEntityView initiator, bool adjustHeight)
	{
		MechanicEntityView mechanicEntityView = projectile?.Target.Entity?.View;
		if (projectile != null && mechanicEntityView == target)
		{
			return projectile.GetTargetPoint();
		}
		if (target == null)
		{
			PFLog.Default.Warning("Trying to spawn hit fx without projectile or target unit");
			return Vector3.zero;
		}
		FxBone hitLocator = GetHitLocator(projectile, initiator, target);
		Vector3 result = ((hitLocator?.Transform != null) ? hitLocator.Transform.position : (target.ViewTransform.position + Vector3.up));
		if (adjustHeight && projectile == null && initiator != null)
		{
			ParticlesSnapMap component = initiator.GetComponent<ParticlesSnapMap>();
			if (component != null)
			{
				FxBone fxBone = component["Locator_TorsoCenterFX"];
				if (fxBone?.Transform != null)
				{
					HitSystemRoot hitSystemRoot = BlueprintRoot.Instance.HitSystemRoot;
					float num = fxBone.Transform.position.y + hitSystemRoot.MaxHeightIncrease;
					if (result.y > num)
					{
						result.y = num;
					}
				}
			}
		}
		if (hitLocator != null && !hitLocator.Transform)
		{
			PFLog.Default.Warning(target, "Bone '" + hitLocator.Name + "' has no transform (unit '" + target.name + "')");
		}
		Transform orientFrom = GetOrientFrom(projectile, initiator);
		if (hitLocator != null && orientFrom != null)
		{
			result += (orientFrom.position - target.ViewTransform.position).normalized * hitLocator.CameraOffset;
		}
		return result;
	}

	[CanBeNull]
	private static FxBone GetHitLocator([CanBeNull] Projectile projectile, [CanBeNull] MechanicEntityView initiator, MechanicEntityView target)
	{
		ParticlesSnapMap particlesSnapMap = target.ParticlesSnapMap;
		if ((projectile == null && initiator == null) || particlesSnapMap == null)
		{
			return null;
		}
		if (initiator == null || (bool)initiator.ParticlesSnapMap)
		{
			return null;
		}
		ParticlesSnapMap particlesSnapMap2 = initiator.ParticlesSnapMap;
		Vector3 vector;
		Vector3 vector2;
		if (projectile != null)
		{
			vector = projectile.LaunchPosition;
			vector2 = projectile.CorePosition - vector;
		}
		else
		{
			Transform transform = ObjectExtensions.Or(particlesSnapMap2, null)?.GetLocators(FxRoot.Instance.LocatorGroupHitter).Random(PFStatefulRandom.Visuals.HitSystem)?.Transform;
			Transform transform2 = ObjectExtensions.Or(particlesSnapMap, null)?.GetLocators(FxRoot.Instance.LocatorGroupTorso).Random(PFStatefulRandom.Visuals.HitSystem)?.Transform;
			if (transform == null || transform2 == null)
			{
				return null;
			}
			vector = transform.position;
			vector2 = transform2.position - vector;
		}
		float num = float.MaxValue;
		FxBone fxBone = null;
		foreach (FxBone bone in particlesSnapMap.Bones)
		{
			Transform transform3 = bone.Transform;
			if (!(transform3 == null))
			{
				Vector3 vector3 = transform3.position - vector;
				Vector3 vector4 = Vector3.Dot(vector3, vector2) / Vector3.Dot(vector2, vector2) * vector2;
				float sqrMagnitude = (vector3 - vector4).sqrMagnitude;
				if (fxBone == null || sqrMagnitude < num)
				{
					num = sqrMagnitude;
					fxBone = bone;
				}
			}
		}
		return fxBone;
	}

	private static Quaternion GetHitFxSpawnRotation([CanBeNull] Projectile projectile, [CanBeNull] MechanicEntityView target, [CanBeNull] MechanicEntityView initiator)
	{
		if (projectile != null)
		{
			return projectile.View.transform.rotation;
		}
		if (initiator != null && initiator is UnitEntityView unitEntityView)
		{
			return unitEntityView.HandsEquipment.GetWeaponRotation();
		}
		return Quaternion.identity;
	}

	[CanBeNull]
	private static Transform GetOrientFrom([CanBeNull] Projectile projectile, [CanBeNull] MechanicEntityView initiator)
	{
		if (projectile != null)
		{
			return projectile.View.transform;
		}
		if (initiator != null)
		{
			return initiator.ViewTransform;
		}
		return null;
	}

	[CanBeNull]
	private static GameObject PlayHit([NotNull] GameObject hit, in Vector3 hitFxSpawnPosition, in Quaternion hitFxSpawnRotation, [CanBeNull] MechanicEntityView attachEntity, bool enableFxObject = true)
	{
		PooledFx pooledFx = hit.GetComponent<PooledFx>();
		if (!pooledFx)
		{
			pooledFx = hit.AddComponent<PooledFx>();
		}
		if (!pooledFx.enabled)
		{
			pooledFx.enabled = true;
		}
		GameObject gameObject = FxHelper.SpawnFxOnPoint(hit, hitFxSpawnPosition, hitFxSpawnRotation, enableFxObject);
		if (attachEntity != null)
		{
			SnapToTransform snapToTransform = gameObject.GetComponent<SnapToTransform>();
			if (snapToTransform == null)
			{
				snapToTransform = gameObject.AddComponent<SnapToTransform>();
			}
			snapToTransform.SetTrackedTransform(attachEntity.ViewTransform);
		}
		return gameObject;
	}

	private static void SpawnVoidShieldHitFx(Projectile projectile)
	{
		Vector3? voidShieldHitPos = projectile.VoidShieldHitPos;
		if (voidShieldHitPos.HasValue)
		{
			MechanicEntity entity = projectile.Target.Entity;
			if (entity == null)
			{
				throw new Exception("Target entity is missing");
			}
			GameObject gameObject = FxHelper.SpawnFxOnPoint(BlueprintRoot.Instance.HitSystemRoot.VoidShieldHit, projectile.View.transform.position, projectile.View.transform.rotation);
			Vector3 position = entity.View.GetComponentInChildren<StarshipView>().transform.position;
			gameObject.transform.forward = (position - voidShieldHitPos.Value).normalized;
		}
	}
}
