using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers;

public class AbilityExecutionProcess : IDisposable
{
	private readonly IEnumerator<object> m_Process;

	private bool m_InstantDeliver;

	private bool m_IsDisposed;

	public AbilityExecutionContext Context { get; }

	public bool IsStarted { get; private set; }

	public bool IsEnded { get; private set; }

	public bool IsEngageUnit => Context.AbilityBlueprint.GetComponent<AbilityDeliverEffect>()?.IsEngageUnit ?? false;

	public AbilityExecutionProcess(AbilityExecutionContext context)
	{
		Context = context;
		m_Process = ProcessRoutine();
	}

	public void Tick()
	{
		TimeSpan gameTime = Game.Instance.Player.GameTime;
		if (IsEnded)
		{
			Game.Instance.Player.GameTime = gameTime;
			PFLog.Default.Error("Ability already ended");
			return;
		}
		using (ContextData<GameLogDisabled>.RequestIf(Context.DisableLog))
		{
			try
			{
				IsEnded = !m_Process.MoveNext();
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				IsEnded = true;
			}
			finally
			{
				Game.Instance.Player.GameTime = gameTime;
			}
		}
		if (IsEnded)
		{
			Context.AbilityBlueprint.CallComponents(delegate(AbilityCustomLogic c)
			{
				c.Cleanup(Context);
			});
		}
	}

	public void Dispose()
	{
		if (m_IsDisposed)
		{
			return;
		}
		m_IsDisposed = true;
		Context.ClearBlockedNodes();
		if (IsStarted && !IsEnded)
		{
			if (Context.MaybeCaster != null)
			{
				EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
				{
					h.HandleExecutionProcessEnd(Context);
				}, isCheckRuntime: true);
			}
			Context.AbilityBlueprint.CallComponents(delegate(AbilityCustomLogic c)
			{
				c.Cleanup(Context);
			});
		}
		IsEnded = true;
		if (Context.MaybeCaster != null)
		{
			EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessClearedHandler>)delegate(IAbilityExecutionProcessClearedHandler h)
			{
				h.HandleExecutionProcessCleared(Context);
			}, isCheckRuntime: true);
		}
	}

	public static void PrepareCast(AbilityExecutionContext context)
	{
		context.Recalculate();
		if (context.Ability.CanRedirectFromTarget(context.ClickedTarget))
		{
			context.RedirectActive = true;
			context.RedirectTargets.AddRange(context.Ability.CalculateRedirectTargets(context.ClickedTarget));
			if (context.RedirectTargets.Empty() && context.Ability.RedirectSettings.CastOnSelfIfNoTargetsAvailable)
			{
				context.RedirectActive = false;
			}
		}
		if (context.RedirectActive)
		{
			PartAbilityRedirect orCreate = context.Caster.GetOrCreate<PartAbilityRedirect>();
			orCreate.LastUsedAbility = context.Ability.Fact;
			orCreate.LastUsedAbilityTarget = context.ClickedTarget.Entity;
			context.Caster.GetOrCreate<UnitPartStatsOverride>().Setup(context.Ability.RedirectSettings?.StatsToOverrideForCaster.ToList(), context.ClickedTarget.Entity);
			context.Recalculate();
		}
		context.AbilityBlueprint.CallComponents(delegate(IAbilityOnCastLogic c)
		{
			c.OnCast(context);
		});
	}

	public static void ApplyEffectImmediate(AbilityExecutionContext context, BaseUnitEntity unit)
	{
		try
		{
			context.DisableFx = true;
			AbilityApplyEffect component = context.AbilityBlueprint.GetComponent<AbilityApplyEffect>();
			DoApplyEffect(context, new AbilityDeliveryTarget(unit), component);
		}
		finally
		{
			context.DisableFx = false;
		}
	}

	private IEnumerator<object> ProcessRoutine()
	{
		IsStarted = true;
		EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
		{
			h.HandleExecutionProcessStart(Context);
		}, isCheckRuntime: true);
		AbilityDeliverEffect component = Context.AbilityBlueprint.GetComponent<AbilityDeliverEffect>();
		BlueprintComponentsEnumerator<AbilityApplyEffect> applyEffectComponents = Context.AbilityBlueprint.GetComponents<AbilityApplyEffect>();
		AbilitySelectTarget selectTargets = Context.AbilityBlueprint.GetComponent<AbilitySelectTarget>();
		PrepareCast(Context);
		SpawnFxs(Context, AbilitySpawnFxTime.OnStart);
		EventBus.RaiseEvent(delegate(IDeliverAbilityEffectHandler h)
		{
			h.OnDeliverAbilityEffect(Context, Context.ClickedTarget);
		});
		if (component != null && !m_InstantDeliver)
		{
			IEnumerator<AbilityDeliveryTarget> deliverProcess = component.Deliver(Context, Context.ClickedTarget);
			IEnumerable<AbilityApplyEffect> applyEffects = ((!Context.RedirectActive) ? applyEffectComponents.AsEnumerable() : null);
			while (TickDeliveryProcess(deliverProcess, Context, selectTargets, applyEffects, isInRedirectProcess: false))
			{
				yield return null;
			}
		}
		else
		{
			AbilityDeliveryTarget deliveryTarget = new AbilityDeliveryTarget(Context.ClickedTarget);
			IEnumerable<AbilityApplyEffect> applyEffects2 = ((!Context.RedirectActive) ? applyEffectComponents.AsEnumerable() : null);
			ApplyEffectHit(Context, deliveryTarget, applyEffects2, selectTargets, m_InstantDeliver);
		}
		AbilityRedirect redirectSettings = Context.Ability.RedirectSettings;
		if (redirectSettings != null && !Context.RedirectTargets.Empty())
		{
			Context.Ability.IsRedirected = true;
			EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessRedirectHandler>)delegate(IAbilityExecutionProcessRedirectHandler h)
			{
				h.HandleAbilityRedirected(Context);
			}, isCheckRuntime: true);
			IEnumerator<AbilityDeliveryTarget>[] deliveryProcessList = new IEnumerator<AbilityDeliveryTarget>[Context.RedirectTargets.Count];
			for (int i = 0; i < Context.RedirectTargets.Count; i++)
			{
				MechanicEntity mechanicEntity = Context.RedirectTargets[i];
				if (!m_InstantDeliver)
				{
					deliveryProcessList[i] = redirectSettings.Deliver(Context, mechanicEntity);
					continue;
				}
				AbilityDeliveryTarget deliveryTarget2 = new AbilityDeliveryTarget(Context.RedirectTargets[i]);
				ApplyEffectHit(Context, deliveryTarget2, applyEffectComponents.AsEnumerable(), selectTargets, m_InstantDeliver);
			}
			while (true)
			{
				bool flag = false;
				for (int j = 0; j < deliveryProcessList.Length; j++)
				{
					IEnumerator<AbilityDeliveryTarget> enumerator = deliveryProcessList[j];
					if (enumerator != null)
					{
						bool flag2 = TickDeliveryProcess(enumerator, Context, selectTargets, applyEffectComponents.AsEnumerable(), isInRedirectProcess: true);
						if (!flag2)
						{
							deliveryProcessList[j] = null;
						}
						flag = flag || flag2;
					}
				}
				if (!flag)
				{
					break;
				}
				yield return null;
			}
		}
		if (Context.Ability.IsWeaponAttackThatRequiresAmmo)
		{
			ItemEntityWeapon sourceWeapon = Context.Ability.SourceWeapon;
			sourceWeapon.CurrentAmmo = Math.Max(sourceWeapon.CurrentAmmo - Context.Ability.AmmoRequired, 0);
		}
		while (Context.ApproachingEffects != null)
		{
			foreach (AbilityApproachingEffect approachingEffect in Context.ApproachingEffects)
			{
				approachingEffect.TickApproaching();
				if (approachingEffect.FinishedApproaching)
				{
					DoApplyEffect(Context, approachingEffect.EffectTarget, approachingEffect.ApplyEffect);
				}
			}
			Context.CleanupApproachingEffects();
			yield return null;
		}
		Context.Caster.GetOptional<UnitPartStatsOverride>()?.Remove();
		EventBus.RaiseEvent((IMechanicEntity)Context.Caster, (Action<IAbilityExecutionProcessHandler>)delegate(IAbilityExecutionProcessHandler h)
		{
			h.HandleExecutionProcessEnd(Context);
		}, isCheckRuntime: true);
	}

	private static bool TickDeliveryProcess(IEnumerator<AbilityDeliveryTarget> deliveryProcess, AbilityExecutionContext context, [CanBeNull] AbilitySelectTarget selectTargets, [CanBeNull] IEnumerable<AbilityApplyEffect> effects, bool isInRedirectProcess)
	{
		try
		{
			bool flag;
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(context.HitPolicy))
			{
				using (ContextData<DamagePolicyContextData>.Request().Setup(context.DamagePolicy))
				{
					while (true)
					{
						TimeSpan? delayBetweenActions = context.DelayBetweenActions;
						if (delayBetweenActions.HasValue)
						{
							double totalSeconds = delayBetweenActions.GetValueOrDefault().TotalSeconds;
							if (totalSeconds >= 0.001)
							{
								double totalSeconds2 = (Game.Instance.TimeController.GameTime - context.CastTime).TotalSeconds;
								int num = Math.Clamp(max: context.Ability.ActionsCount, value: (int)(totalSeconds2 / totalSeconds) + 1, min: 1);
								while (context.ActionIndex < num)
								{
									context.NextAction();
								}
							}
						}
						using (context.GetDataScope())
						{
							using (ProfileScope.New("TickDelivery"))
							{
								flag = deliveryProcess.MoveNext();
							}
						}
						if (flag && deliveryProcess.Current != null)
						{
							if (!isInRedirectProcess && context.RedirectActive && deliveryProcess.Current.Target == context.ClickedTarget)
							{
								flag = false;
								break;
							}
							using (ProfileScope.New("ApplyEffect"))
							{
								ApplyEffect(context, deliveryProcess.Current, effects, selectTargets);
							}
							EventBus.RaiseEvent(delegate(IDeliverAbilityEffectHandler h)
							{
								h.OnDeliverAbilityEffect(context, deliveryProcess.Current.Target);
							});
							continue;
						}
						break;
					}
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
	}

	private static void SpawnFxs([NotNull] AbilityExecutionContext context, AbilitySpawnFxTime time, [CanBeNull] TargetWrapper selectedTarget = null)
	{
		foreach (AbilitySpawnFx fxSpawner in context.FxSpawners)
		{
			if (fxSpawner.Time == time)
			{
				fxSpawner.Spawn(context, selectedTarget);
			}
		}
	}

	private static void ApplyEffect(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffect, [CanBeNull] AbilitySelectTarget selectTargets)
	{
		if (context.AbilityBlueprint.GetComponent<AbilityEffectMissIsHit>() != null || deliveryTarget.AttackRule == null || deliveryTarget.AttackRule.ResultIsHit)
		{
			ApplyEffectHit(context, deliveryTarget, applyEffect, selectTargets, instant: false);
		}
	}

	public static void ApplyEffectHit(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffects, [CanBeNull] AbilitySelectTarget selectTargets, bool instant)
	{
		if (selectTargets != null)
		{
			float? spreadSpeedMps = selectTargets.GetSpreadSpeedMps();
			int targetsInPattern = selectTargets.Select(context, deliveryTarget.Target).Count();
			using (ContextData<TargetsInPatternData>.Request().Setup(targetsInPattern))
			{
				foreach (TargetWrapper item in selectTargets.Select(context, deliveryTarget.Target))
				{
					AbilityDeliveryTarget abilityDeliveryTarget = new AbilityDeliveryTarget(item, deliveryTarget);
					if (instant)
					{
						DoApplyEffect(context, abilityDeliveryTarget, applyEffects);
					}
					else
					{
						ScheduleEffect(context, deliveryTarget, abilityDeliveryTarget, applyEffects, spreadSpeedMps);
					}
				}
			}
		}
		else
		{
			DoApplyEffect(context, deliveryTarget, applyEffects);
		}
		SpawnFxs(context, AbilitySpawnFxTime.OnApplyEffect);
		EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
		{
			h.OnAbilityEffectApplied(context);
		});
	}

	private static void ScheduleEffect(AbilityExecutionContext context, AbilityDeliveryTarget deliveryTarget, AbilityDeliveryTarget effectTarget, IEnumerable<AbilityApplyEffect> applyEffects, float? spreadSpeedMps)
	{
		if (!spreadSpeedMps.HasValue || (deliveryTarget.Target.Point - effectTarget.Target.Point).sqrMagnitude <= 0.0001f)
		{
			DoApplyEffect(context, effectTarget, applyEffects);
			return;
		}
		foreach (AbilityApplyEffect applyEffect in applyEffects)
		{
			AbilityApproachingEffect effect = new AbilityApproachingEffect(effectTarget, deliveryTarget, applyEffect, spreadSpeedMps.Value);
			context.AddApproachingEffect(effect);
		}
	}

	private static void DoApplyEffect(AbilityExecutionContext context, AbilityDeliveryTarget target, [CanBeNull] IEnumerable<AbilityApplyEffect> applyEffects)
	{
		if (applyEffects == null || applyEffects.Empty())
		{
			DoApplyEffect(context, target, (AbilityApplyEffect)null);
			return;
		}
		foreach (AbilityApplyEffect applyEffect in applyEffects)
		{
			DoApplyEffect(context, target, applyEffect);
		}
	}

	private static void DoApplyEffect(AbilityExecutionContext context, AbilityDeliveryTarget target, [CanBeNull] AbilityApplyEffect applyEffect)
	{
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error(context.AbilityBlueprint, "Caster is missing");
			return;
		}
		using (ContextData<DamagePolicyContextData>.Request().Setup(context.DamagePolicy))
		{
			EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
			{
				h.OnTryToApplyAbilityEffect(context, target);
			});
			using (context.GetDataScope(target.Target))
			{
				using (AbilityExecutionContext.GetAbilityDataScope(target.AttackRule, target.Projectile, context.ClickedTarget))
				{
					applyEffect?.Apply(context, target.Target);
					foreach (BlueprintAbilityAdditionalEffect additionalEffect in context.Ability.AdditionalEffects)
					{
						using (new MechanicsContext(context.Caster, context.MaybeOwner, additionalEffect, context, context.MainTarget).GetDataScope(target.Target))
						{
							additionalEffect.OnHitActions.Run();
						}
					}
				}
			}
			SpawnFxs(context, AbilitySpawnFxTime.OnApplyEffect, target.Target);
			EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
			{
				h.OnAbilityEffectAppliedToTarget(context, target);
			});
		}
	}

	public void InstantDeliver()
	{
		m_InstantDeliver = true;
	}

	public void Detach()
	{
		Game.Instance.AbilityExecutor.Detach(this);
	}
}
