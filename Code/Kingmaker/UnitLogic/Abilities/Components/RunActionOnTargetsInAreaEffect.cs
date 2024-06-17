using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowMultipleComponents]
[TypeId("b2438912ccbf4301a1935c4c9a87e6e1")]
public class RunActionOnTargetsInAreaEffect : AbilityApplyEffect
{
	public bool Not;

	[SerializeField]
	private List<BlueprintAbilityAreaEffectReference> m_AreaEffectsWhiteList = new List<BlueprintAbilityAreaEffectReference>();

	[CanBeNull]
	public ActionList Actions;

	[CanBeNull]
	public ActionList ActionsOnCaster;

	[CanBeNull]
	public ActionList ActionsOnDestructibleEntities;

	[CanBeNull]
	public ActionList ActionsOnTargetPoint;

	public override void Apply(AbilityExecutionContext context, TargetWrapper target)
	{
		if (target == null)
		{
			return;
		}
		if (m_AreaEffectsWhiteList.Empty())
		{
			throw new Exception("RunActionOnTargetsInAreaEffect: WhiteList is empty, won't run any actions");
		}
		CustomGridNodeBase nearestNodeXZ = target.Point.GetNearestNodeXZ();
		foreach (AreaEffectEntity areaEffect in Game.Instance.State.AreaEffects)
		{
			if (!areaEffect.Contains(nearestNodeXZ) || ((Not || !m_AreaEffectsWhiteList.Contains(areaEffect.Blueprint.ToReference<BlueprintAbilityAreaEffectReference>())) && (!Not || m_AreaEffectsWhiteList.Contains(areaEffect.Blueprint.ToReference<BlueprintAbilityAreaEffectReference>()))))
			{
				continue;
			}
			ActionList actions = Actions;
			if (actions != null && actions.HasActions)
			{
				foreach (BaseUnitEntity item in areaEffect.InGameUnitsInside)
				{
					using (context.GetDataScope(item.ToITargetWrapper()))
					{
						Actions.Run();
					}
				}
			}
			actions = ActionsOnCaster;
			if (actions != null && actions.HasActions)
			{
				using (context.GetDataScope(context.Caster.ToITargetWrapper()))
				{
					ActionsOnCaster.Run();
				}
			}
			actions = ActionsOnDestructibleEntities;
			if (actions != null && actions.HasActions)
			{
				DestructibleEntity[] allDestructibleEntityInside = areaEffect.GetAllDestructibleEntityInside();
				foreach (DestructibleEntity entity in allDestructibleEntityInside)
				{
					using (context.GetDataScope(entity.ToITargetWrapper()))
					{
						ActionsOnDestructibleEntities.Run();
					}
				}
			}
			actions = ActionsOnTargetPoint;
			if (actions != null && actions.HasActions)
			{
				using (context.GetDataScope(target))
				{
					ActionsOnTargetPoint.Run();
				}
			}
		}
	}
}
