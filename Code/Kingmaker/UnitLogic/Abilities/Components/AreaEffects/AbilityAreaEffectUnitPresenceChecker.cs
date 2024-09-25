using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[TypeId("9bfada3e8bee43f2aa4bfad9f7627778")]
public class AbilityAreaEffectUnitPresenceChecker : AbilityAreaEffectLogic
{
	public TargetType CheckForNoTargetsOfType;

	public ActionList ActionsOnAllUnitsInside;

	[SerializeField]
	private BlueprintAbilityAreaEffectReference m_NewAreaEffect;

	public ActionList ActionsOnAllUnitsInsideIfFailed;

	public BlueprintAbilityAreaEffect NewAreaEffect => m_NewAreaEffect.Get();

	protected override void OnUnitExit(MechanicsContext context, AreaEffectEntity areaEffect, BaseUnitEntity unit)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			return;
		}
		if (CheckTargetType(maybeCaster, unit, CheckForNoTargetsOfType))
		{
			foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
			{
				if (CheckTargetType(maybeCaster, item, CheckForNoTargetsOfType) && !item.LifeState.IsDead && item != unit)
				{
					return;
				}
				if (item.LifeState.IsDead)
				{
					continue;
				}
				using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
				{
					using (context.GetDataScope(item.ToITargetWrapper()))
					{
						ActionsOnAllUnitsInside.Run();
					}
				}
			}
		}
		if (NewAreaEffect != null)
		{
			TargetWrapper target = new TargetWrapper(areaEffect.Position);
			AreaEffectEntity areaEffectEntity = AreaEffectsController.Spawn(context, NewAreaEffect, target, 5.Rounds().Seconds);
			if (areaEffectEntity != null)
			{
				foreach (BaseUnitEntity u in Game.Instance.State.AllBaseUnits)
				{
					if (u.LifeState.IsDead || !u.IsInGame || areaEffectEntity.Blueprint.IsAllArea || !areaEffectEntity.Contains(u))
					{
						continue;
					}
					if (!areaEffectEntity.AffectEnemies)
					{
						MechanicEntity maybeCaster2 = context.MaybeCaster;
						if (maybeCaster2 == null || maybeCaster2.IsEnemy(u))
						{
							continue;
						}
					}
					EventBus.RaiseEvent(delegate(IApplyAbilityEffectHandler h)
					{
						h.OnTryToApplyAbilityEffect(context.SourceAbilityContext, new AbilityDeliveryTarget(u));
					});
				}
			}
		}
		areaEffect.ForceEnd();
	}

	protected override void OnEndForEachUnit(MechanicsContext context, AreaEffectEntity areaEffect)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			return;
		}
		foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
		{
			if (CheckTargetType(maybeCaster, item, CheckForNoTargetsOfType) || item.LifeState.IsDead)
			{
				continue;
			}
			using (ContextData<AreaEffectContextData>.Request().Setup(areaEffect))
			{
				using (context.GetDataScope(item.ToITargetWrapper()))
				{
					ActionsOnAllUnitsInsideIfFailed.Run();
				}
			}
		}
	}

	public static bool CheckTargetType(MechanicEntity caster, MechanicEntity target, TargetType targetType)
	{
		if (caster == null || target == null)
		{
			return false;
		}
		if (targetType == TargetType.Any)
		{
			return true;
		}
		PartCombatGroup combatGroupOptional = target.GetCombatGroupOptional();
		if (targetType == TargetType.Ally && combatGroupOptional != null && combatGroupOptional.IsAlly(caster))
		{
			return true;
		}
		if (targetType == TargetType.Enemy && combatGroupOptional != null && combatGroupOptional.IsEnemy(caster))
		{
			return true;
		}
		return false;
	}
}
