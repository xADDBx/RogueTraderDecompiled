using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.FX;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Enums.Sound;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound;
using Kingmaker.Sound.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Visual.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.Animation.Kingmaker.Actions;
using Kingmaker.Visual.FX;
using Kingmaker.Visual.HitSystem;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Controllers;

public class VisualEffectsController : IController, IAnimationEventHandler, ISubscriber<IMechanicEntity>, ISubscriber, IAbilityExecutionProcessHandler, IApplyAbilityEffectHandler, IProjectileLaunchedHandler, IProjectileHitHandler, IDamageFXHandler, IDodgeHandler, IAreaEffectHandler, ISubscriber<IAreaEffectEntity>, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IGlobalRulebookSubscriber, IBuffEffectHandler, IUnitCommandStartHandler
{
	private static void TryPlayVisualFX([NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [CanBeNull] AbilityData ability, MappedAnimationEventType? animationEvent, AbilityEventType? abilityEvent)
	{
		if (ability == null)
		{
			ability = (caster.GetCommandsOptional()?.Current as UnitUseAbility)?.Ability;
		}
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = ability?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings != null)
		{
			if (animationEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, animationEvent.Value, ability);
			}
			if (abilityEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, target ?? ((TargetWrapper)caster), abilityEvent.Value, ability);
			}
		}
	}

	private static void TryPlaySoundFX([NotNull] AbilityExecutionContext context, [CanBeNull] TargetWrapper target, [CanBeNull] AbilityData ability, AbilityEventType abilityEvent)
	{
		if (ability == null)
		{
			ability = context.Caster.GetCommandsOptional()?.GetCurrent<UnitUseAbility>()?.Ability;
		}
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = ability?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, context, abilityEvent);
		}
	}

	private static void TryPlaySoundFX([NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, [CanBeNull] AbilityData ability, AbilityEventType abilityEvent)
	{
		if (ability == null)
		{
			ability = caster.GetCommandsOptional()?.GetCurrent<UnitUseAbility>()?.Ability;
		}
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = ability?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, caster, target, abilityEvent);
		}
	}

	private static GameObject[] TryPlayBuffFX([NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [NotNull] IBuff buff, MappedAnimationEventType? animationEvent, AbilityEventType? abilityEvent)
	{
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = buff?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings == null)
		{
			return Array.Empty<GameObject>();
		}
		if (animationEvent.HasValue)
		{
			FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, animationEvent.Value);
		}
		if (abilityEvent.HasValue)
		{
			return FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, target ?? ((TargetWrapper)caster), abilityEvent.Value);
		}
		return Array.Empty<GameObject>();
	}

	private static void TryPlaySoundFX([NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, [NotNull] IBuff buff, AbilityEventType buffEvent)
	{
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = buff?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, caster, target.Entity ?? caster, buffEvent);
		}
	}

	private static void TryPlayAreaEffectFX([NotNull] MechanicEntity caster, [CanBeNull] TargetWrapper target, [NotNull] AreaEffectEntity areaEffect, MappedAnimationEventType? animationEvent, AbilityEventType? abilityEvent)
	{
		BlueprintAbilityVisualFXSettings blueprintAbilityVisualFXSettings = areaEffect?.FXSettings?.VisualFXSettings;
		if (blueprintAbilityVisualFXSettings != null)
		{
			if (animationEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, animationEvent.Value);
			}
			if (abilityEvent.HasValue)
			{
				FXPlayer.Play(blueprintAbilityVisualFXSettings, caster, target ?? ((TargetWrapper)caster), abilityEvent.Value);
			}
		}
	}

	private static void TryPlaySoundFX([NotNull] MechanicsContext context, [CanBeNull] TargetWrapper target, [NotNull] AreaEffectEntity areaEffect, AbilityEventType abilityEvent)
	{
		BlueprintAbilitySoundFXSettings blueprintAbilitySoundFXSettings = areaEffect?.FXSettings?.SoundFXSettings;
		if (blueprintAbilitySoundFXSettings != null)
		{
			SoundEventPlayer.Play(blueprintAbilitySoundFXSettings, context, abilityEvent);
		}
	}

	private static void TryPlayVisualFX([NotNull] MechanicEntity caster, MappedAnimationEventType eventType)
	{
		TryPlayVisualFX(caster, null, null, eventType, null);
	}

	private static void TryPlayVisualFX([NotNull] MechanicEntity caster, [NotNull] TargetWrapper target, [NotNull] AbilityData ability, AbilityEventType eventType)
	{
		TryPlayVisualFX(caster, target, ability, null, eventType);
	}

	private static void TryPlayProjectileFX(Projectile projectile, TargetWrapper target, AbilityEventType eventType)
	{
		MechanicEntity entity = projectile.Launcher.Entity;
		AbilityData ability = projectile.Ability;
		if (entity != null && !(ability == null))
		{
			TryPlayVisualFX(entity, target, ability, eventType);
			TryPlaySoundFX(entity, target, ability, eventType);
			if (eventType == AbilityEventType.ProjectileHit)
			{
				HitFXPlayer.PlayProjectileHit(projectile, target);
			}
		}
	}

	private static void TryPlayDamageFx(RuleDealDamage rule)
	{
		HitFXPlayer.PlayDamageHit(rule);
	}

	private static void TryPlayAnimationHit(RuleDealDamage rule)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)rule.Target;
		if ((object)mechanicEntity.View?.EntityData.MaybeAnimationManager != null)
		{
			UnitAnimationActionHandle unitAnimationActionHandle = mechanicEntity.View.EntityData.MaybeAnimationManager.CreateHandle(UnitAnimationType.Hit, errorOnEmpty: false);
			if (unitAnimationActionHandle != null && !(mechanicEntity.View.EntityData.MaybeAnimationManager.CurrentAction.Action is UnitAnimationActionJump))
			{
				mechanicEntity.View.EntityData.MaybeAnimationManager.Execute(unitAnimationActionHandle);
			}
		}
	}

	private static void TryPlayHitEffect(Projectile projectile, TargetWrapper target)
	{
		if (projectile.Hits.Length != 0 && !projectile.DoNotPlayHitEffect)
		{
			if (projectile.IsCoverHit)
			{
				SoundEventsManager.PostEvent(BlueprintRoot.Instance.FxRoot.InCoverHitSoundEvent, projectile.ClosestHit.transform.gameObject);
			}
			Vector3 projectileHitFxSpawnPosition = HitFXPlayer.GetProjectileHitFxSpawnPosition(projectile, target);
			SphereBounds hitFxCullingSphere = new SphereBounds(projectileHitFxSpawnPosition, 0.5f);
			SurfaceHitController.Instance.ProcessProjectileHits(projectile, in hitFxCullingSphere);
		}
	}

	void IAnimationEventHandler.HandleAnimationEvent(MappedAnimationEventType eventType)
	{
		TryPlayVisualFX(EventInvokerExtensions.MechanicEntity, eventType);
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		TryPlayVisualFX(context.Caster, context.MainTarget, context.Ability, AbilityEventType.Start);
		TryPlaySoundFX(context, context.MainTarget, context.Ability, AbilityEventType.Start);
	}

	void IAbilityExecutionProcessHandler.HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		TryPlayVisualFX(context.Caster, context.MainTarget, context.Ability, AbilityEventType.End);
		TryPlaySoundFX(context, context.MainTarget, context.Ability, AbilityEventType.End);
	}

	void IApplyAbilityEffectHandler.OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		TryPlayVisualFX(context.Caster, target.Target, context.Ability, AbilityEventType.HitTarget);
		TryPlaySoundFX(context, target.Target, context.Ability, AbilityEventType.HitTarget);
	}

	void IApplyAbilityEffectHandler.OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	void IApplyAbilityEffectHandler.OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	void IProjectileLaunchedHandler.HandleProjectileLaunched(Projectile projectile)
	{
		TryPlayProjectileFX(projectile, projectile.Target, AbilityEventType.ProjectileLaunched);
	}

	void IProjectileHitHandler.HandleProjectileHit(Projectile projectile)
	{
		TryPlayHitEffect(projectile, projectile.Target);
		TryPlayProjectileFX(projectile, projectile.Target, AbilityEventType.ProjectileHit);
	}

	void IDamageFXHandler.HandleDamageDealt(RuleDealDamage dealDamage)
	{
		MechanicEntity mechanicEntity = (MechanicEntity)dealDamage.Target;
		TryPlayDamageFx(dealDamage);
		IMechanicEntity target = dealDamage.Target;
		UnitEntity unitEntity = target as UnitEntity;
		if (unitEntity != null)
		{
			if (mechanicEntity.View == null)
			{
				return;
			}
			ShieldHitEntry shieldHitEntry = BlueprintRoot.Instance.HitSystemRoot.ShieldHitEntrys.FirstOrDefault((ShieldHitEntry x) => x.Type == unitEntity.ShieldType);
			bool flag = shieldHitEntry != null;
			bool flag2 = !flag || shieldHitEntry.ShowImpact;
			bool flag3 = !flag || shieldHitEntry.HitSoundEvent;
			if (flag)
			{
				if (shieldHitEntry.ShowHitAnimation)
				{
					TryPlayAnimationHit(dealDamage);
				}
				if (shieldHitEntry.ShowSpecialEffect)
				{
					if (shieldHitEntry.HitInSphere)
					{
						if (dealDamage.ConcreteTarget.View != null)
						{
							Vector3 vector = -((dealDamage.TargetUnit?.Position - dealDamage?.Projectile?.Launcher.Point) ?? Vector3.zero).normalized * (shieldHitEntry.SphereRadius * (dealDamage.ConcreteTarget.View.ParticlesSnapMap?.SizeScale ?? 1f));
							GameObject gameObject = FxHelper.SpawnFxOnPoint(shieldHitEntry.SpecialEffect, dealDamage.ConcreteTarget.View.transform.position + Vector3.up + vector, Quaternion.FromToRotation(Vector3.forward, vector.normalized), !shieldHitEntry.OverrideTargetTypeSwitch);
							if (shieldHitEntry.OverrideTargetTypeSwitch)
							{
								if (gameObject.TryGetComponent<SoundFx>(out var component))
								{
									component.BlockSoundFXPlaying = true;
								}
								gameObject.SetActive(value: true);
							}
						}
					}
					else
					{
						GameObject gameObject2 = FxHelper.SpawnFxOnEntity(shieldHitEntry.SpecialEffect, dealDamage.ConcreteTarget.View, !shieldHitEntry.OverrideTargetTypeSwitch);
						if (shieldHitEntry.OverrideTargetTypeSwitch)
						{
							if (gameObject2.TryGetComponent<SoundFx>(out var component2))
							{
								component2.BlockSoundFXPlaying = true;
							}
							gameObject2.SetActive(value: true);
						}
					}
				}
			}
			else
			{
				TryPlayAnimationHit(dealDamage);
			}
			if (flag2)
			{
				FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkEffect.Load(), dealDamage.ConcreteTarget.View);
			}
			if (flag3 && (!(dealDamage.Reason.Fact is Buff buff) || !buff.Blueprint.PlayOnlyFirstHitSound || !buff.PlayedFirstHitSound))
			{
				AkSoundEngine.SetSwitch("HitMainType", dealDamage.ResultIsCritical ? "Crit" : "Normal", dealDamage.ConcreteTarget.View.gameObject);
				AkSwitchReference akSwitchReference = ((flag && shieldHitEntry.OverrideTargetTypeSwitch) ? shieldHitEntry.TargetTypeSwitch : BlueprintRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry x) => x.Type == unitEntity.Blueprint.VisualSettings.SurfaceType)?.Switch);
				if (akSwitchReference.IsValid())
				{
					AkSoundEngine.SetSwitch(akSwitchReference.Group, akSwitchReference.Value, unitEntity.View.gameObject);
				}
				AkSwitchReference akSwitchReference2 = ((flag && shieldHitEntry.OverrideTargetTypeSwitch) ? shieldHitEntry.MuffledTypeSwitch : null);
				if (akSwitchReference2.IsValid())
				{
					AkSoundEngine.SetSwitch(akSwitchReference2.Group, akSwitchReference2.Value, unitEntity.View.gameObject);
				}
				if (dealDamage.Reason.Fact is Buff buff2)
				{
					buff2.PlayedFirstHitSound = true;
					AkSwitchReference soundTypeSwitch = buff2.Blueprint.SoundTypeSwitch;
					if (soundTypeSwitch.IsValid())
					{
						AkSoundEngine.SetSwitch(soundTypeSwitch.Group, soundTypeSwitch.Value, unitEntity.View.gameObject);
					}
					AkSwitchReference muffledTypeSwitch = buff2.Blueprint.MuffledTypeSwitch;
					if (muffledTypeSwitch.IsValid())
					{
						AkSoundEngine.SetSwitch(muffledTypeSwitch.Group, muffledTypeSwitch.Value, unitEntity.View.gameObject);
					}
				}
				else
				{
					try
					{
						AkSwitchReference akSwitchReference3 = dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters.SoundTypeSwitch;
						if (akSwitchReference3.IsValid())
						{
							AkSoundEngine.SetSwitch(akSwitchReference3.Group, akSwitchReference3.Value, unitEntity.View.gameObject);
						}
						AkSwitchReference akSwitchReference4 = dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters.MuffledTypeSwitch;
						if (akSwitchReference4.IsValid())
						{
							AkSoundEngine.SetSwitch(akSwitchReference4.Group, akSwitchReference4.Value, unitEntity.View.gameObject);
						}
					}
					catch
					{
						PFLog.Default.Error($"{dealDamage.SourceAbility?.Weapon} don't have sound type switch");
					}
				}
				SoundEventPlayer.PlaySound(BlueprintRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkSoundSettings, dealDamage.ConcreteTarget.View.gameObject);
			}
		}
		target = dealDamage.Target;
		DestructibleEntity destructibleEntity = target as DestructibleEntity;
		if (destructibleEntity != null)
		{
			HitEntry hitEntry = BlueprintRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry x) => x.Type == destructibleEntity.SurfaceType);
			if (hitEntry != null)
			{
				FxHelper.SpawnFxOnEntity(hitEntry.StaticHitEffects.HitEffectLink.Load(), mechanicEntity.View);
				AkSoundEngine.SetSwitch("HitMainType", dealDamage.ResultIsCritical ? "Crit" : "Normal", mechanicEntity.View.gameObject);
				AkSoundEngine.SetSwitch(hitEntry.Switch.Group, hitEntry.Switch.Value, destructibleEntity.View.gameObject);
				try
				{
					AkSwitchReference akSwitchReference5 = dealDamage.SourceAbility?.Weapon?.Blueprint.VisualParameters.SoundTypeSwitch;
					if (akSwitchReference5.IsValid())
					{
						AkSoundEngine.SetSwitch(akSwitchReference5.Group, akSwitchReference5.Value, destructibleEntity.View.gameObject);
					}
				}
				catch
				{
					PFLog.Default.Error($"{dealDamage.SourceAbility?.Weapon} don't have sound type switch");
				}
				SoundEventPlayer.PlaySound(BlueprintRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkSoundSettings, mechanicEntity.View.gameObject);
			}
		}
		target = dealDamage.Target;
		StarshipEntity starshipEntity = target as StarshipEntity;
		if (starshipEntity == null)
		{
			return;
		}
		HitEntry hitEntry2 = BlueprintRoot.Instance.HitSystemRoot.HitEffects.FirstOrDefault((HitEntry x) => x.Type == starshipEntity.SurfaceType);
		if (hitEntry2 == null)
		{
			return;
		}
		FxHelper.SpawnFxOnEntity(hitEntry2.StaticHitEffects.HitEffectLink.Load(), dealDamage.ConcreteTarget.View);
		AkSoundEngine.SetSwitch("HitMainType", dealDamage.ResultIsCritical ? "Crit" : "Normal", dealDamage.ConcreteTarget.View.gameObject);
		AkSoundEngine.SetSwitch(hitEntry2.Switch.Group, hitEntry2.Switch.Value, starshipEntity.View.gameObject);
		try
		{
			AkSwitchReference akSwitchReference6 = dealDamage.Reason.Ability?.StarshipWeapon?.Blueprint.SoundTypeSwitch;
			if (akSwitchReference6.IsValid())
			{
				AkSoundEngine.SetSwitch(akSwitchReference6.Group, akSwitchReference6.Value, starshipEntity.View.gameObject);
			}
		}
		catch
		{
			PFLog.Default.Error($"{dealDamage.SourceAbility?.Weapon} don't have sound type switch");
		}
		SoundEventPlayer.PlaySound(BlueprintRoot.Instance.HitSystemRoot.GlobalHitEffect.HitMarkSoundSettings, dealDamage.ConcreteTarget.View.gameObject);
	}

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
	}

	public void HandleJumpAsideDodge(RulePerformDodge dodgeRule)
	{
		FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.FxRoot.JumpAsideDodgeFX.Load(), dodgeRule.Defender.View);
	}

	public void HandleSimpleDodge(RulePerformDodge dodgeRule)
	{
		FxHelper.SpawnFxOnEntity(BlueprintRoot.Instance.FxRoot.SimpleDodgeFX.Load(), dodgeRule.Defender.View);
		if ((object)dodgeRule.Defender.MaybeAnimationManager != null)
		{
			UnitAnimationActionHandle handle = dodgeRule.Defender.MaybeAnimationManager.CreateHandle(UnitAnimationType.Dodge);
			dodgeRule.Defender.MaybeAnimationManager.Execute(handle);
		}
	}

	public void HandleAreaEffectSpawned()
	{
		if (EventInvokerExtensions.Entity is AreaEffectEntity { Context: { MaybeCaster: not null } context } areaEffectEntity)
		{
			TryPlayAreaEffectFX(context.MaybeCaster, context.MainTarget, areaEffectEntity, null, AbilityEventType.Start);
			TryPlaySoundFX(context, context.MainTarget, areaEffectEntity, AbilityEventType.Start);
		}
	}

	public void HandleAreaEffectDestroyed()
	{
		if (!(EventInvokerExtensions.Entity is AreaEffectEntity areaEffectEntity))
		{
			return;
		}
		AbilityExecutionContext sourceAbilityContext = areaEffectEntity.Context.SourceAbilityContext;
		if (sourceAbilityContext != null)
		{
			TryPlayVisualFX(sourceAbilityContext.Caster, sourceAbilityContext.MainTarget, sourceAbilityContext.Ability, AbilityEventType.EndAreaEffect);
			TryPlaySoundFX(sourceAbilityContext, sourceAbilityContext.MainTarget, sourceAbilityContext.Ability, AbilityEventType.EndAreaEffect);
			TryPlayAreaEffectFX(sourceAbilityContext.Caster, sourceAbilityContext.MainTarget, areaEffectEntity, null, AbilityEventType.EndAreaEffect);
			TryPlaySoundFX(sourceAbilityContext, sourceAbilityContext.MainTarget, areaEffectEntity, AbilityEventType.EndAreaEffect);
		}
		else if (areaEffectEntity.Context != null)
		{
			if (areaEffectEntity.Context.MaybeCaster != null)
			{
				TryPlayAreaEffectFX(areaEffectEntity.Context.MaybeCaster, areaEffectEntity.Context.MainTarget, areaEffectEntity, null, AbilityEventType.EndAreaEffect);
			}
			TryPlaySoundFX(areaEffectEntity.Context, areaEffectEntity.Context.MainTarget, areaEffectEntity, AbilityEventType.EndAreaEffect);
		}
	}

	public GameObject[] OnBuffEffectApplied(IBuff buff)
	{
		MechanicEntity caster = buff.Caster;
		TargetWrapper target = buff.Target;
		TryPlaySoundFX(caster, target, buff, AbilityEventType.Start);
		return TryPlayBuffFX(caster, target, buff, null, AbilityEventType.Start);
	}

	public void OnBuffEffectRemoved(IBuff buff)
	{
		MechanicEntity caster = buff.Caster;
		TargetWrapper target = buff.Target;
		TryPlaySoundFX(caster, target, buff, AbilityEventType.End);
		TryPlayBuffFX(caster, target, buff, null, AbilityEventType.End);
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		if (command is UnitUseAbility unitUseAbility && unitUseAbility.Target != null)
		{
			TryPlayVisualFX(unitUseAbility.Executor, unitUseAbility.Target, unitUseAbility.Ability, AbilityEventType.StarUseAbilityCommand);
		}
	}
}
