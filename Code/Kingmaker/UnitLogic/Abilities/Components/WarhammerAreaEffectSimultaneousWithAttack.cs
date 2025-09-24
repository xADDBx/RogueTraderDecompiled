using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("cef37152812a4ba9b58d62fa76bc252c")]
public class WarhammerAreaEffectSimultaneousWithAttack : BlueprintComponent
{
	public BlueprintAbilityAreaEffectReference AreaEffect;

	public bool OverridePatternWithAttackPattern;

	public ContextDurationValue DurationValue;

	[SerializeField]
	private bool m_GetOrientationFromCaster;

	[SerializeField]
	private RestrictionCalculator RestrictionsToSpawn;

	private BlueprintAbilityAreaEffect BlueprintAbilityAreaEffect => AreaEffect.Get();

	public void SpawnAreaEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		if (!RestrictionsToSpawn.IsPassed(context, context.Caster))
		{
			return;
		}
		TimeSpan seconds = DurationValue.Calculate(context).Seconds;
		AreaEffectEntity areaEffectEntity = AreaEffectsController.Spawn(overridenPattern: new OverrideAreaEffectPatternData(context.Pattern, OverridePatternWithAttackPattern), parentContext: context, blueprint: BlueprintAbilityAreaEffect, target: target, duration: seconds, getOrientationFromCaster: m_GetOrientationFromCaster);
		if (areaEffectEntity == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.State.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || !context.Caster.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(context, new AbilityDeliveryTarget(u));
				});
			}
		}
	}
}
