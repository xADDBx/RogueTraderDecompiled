using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("4e5ac5e97bccb29429a528734d2051b2")]
public class ContextActionSpawnAreaEffect : ContextAction
{
	[SerializeField]
	[FormerlySerializedAs("AreaEffect")]
	private BlueprintAbilityAreaEffectReference m_AreaEffect;

	public ContextDurationValue DurationValue;

	public bool OnUnit;

	public BlueprintAbilityAreaEffect AreaEffect => m_AreaEffect?.Get();

	public override string GetCaption()
	{
		string arg = ((AreaEffect != null) ? AreaEffect.ToString() : "<undefined>");
		return $"Spawn {arg} for {DurationValue}";
	}

	public override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.PreviewUnit>.Current)
		{
			return;
		}
		TimeSpan seconds = DurationValue.Calculate(base.Context).Seconds;
		AreaEffectEntity areaEffectEntity = (OnUnit ? AreaEffectsController.SpawnAttachedToTarget(base.Context, AreaEffect, (BaseUnitEntity)base.Target.Entity, seconds) : AreaEffectsController.Spawn(base.Context, AreaEffect, base.Target, seconds));
		if (areaEffectEntity == null || base.AbilityContext == null)
		{
			return;
		}
		foreach (BaseUnitEntity u in Game.Instance.State.AllBaseUnits)
		{
			if (!u.LifeState.IsDead && u.IsInGame && !areaEffectEntity.Blueprint.IsAllArea && areaEffectEntity.Contains(u) && (areaEffectEntity.AffectEnemies || !base.AbilityContext.Caster.IsEnemy(u)))
			{
				EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
				{
					h.OnTryToApplyAbilityEffect(base.AbilityContext, new AbilityDeliveryTarget(u));
				});
			}
		}
	}
}
