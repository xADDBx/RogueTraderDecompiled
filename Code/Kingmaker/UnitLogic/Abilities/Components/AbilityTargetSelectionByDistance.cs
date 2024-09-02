using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("e13a55b3d34a43e0810e81a0bf3685ef")]
public class AbilityTargetSelectionByDistance : AbilitySelectTarget
{
	private enum DistanceSettings
	{
		Closest,
		Farthest
	}

	[SerializeField]
	private TargetType m_TargetType;

	[SerializeField]
	private bool m_IncludeCaster;

	[SerializeField]
	private ConditionsChecker m_Condition;

	[SerializeField]
	private DistanceSettings m_DistanceSetting;

	public TargetType Targets => m_TargetType;

	public override IEnumerable<TargetWrapper> Select(AbilityExecutionContext context, TargetWrapper anchor)
	{
		IEnumerable<BaseUnitEntity> enumerable = Game.Instance.State.AllBaseUnits.Where((BaseUnitEntity p) => !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat);
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return Enumerable.Empty<TargetWrapper>();
		}
		switch (Targets)
		{
		case TargetType.Enemy:
			enumerable = enumerable.Where(context.Caster.IsEnemy);
			break;
		case TargetType.Ally:
			enumerable = enumerable.Where(context.Caster.IsAlly);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case TargetType.Any:
			break;
		}
		if (m_Condition.HasConditions)
		{
			enumerable = enumerable.Where(delegate(BaseUnitEntity u)
			{
				using (context.GetDataScope(u.ToITargetWrapper()))
				{
					return m_Condition.Check();
				}
			}).ToList();
		}
		if (enumerable.Empty())
		{
			return Enumerable.Empty<TargetWrapper>();
		}
		return m_DistanceSetting switch
		{
			DistanceSettings.Closest => ConvertToIEnumerable(GetClosestUnit(context, enumerable)), 
			DistanceSettings.Farthest => ConvertToIEnumerable(GetFarthestUnit(context, enumerable)), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static BaseUnitEntity GetClosestUnit(AbilityExecutionContext context, IEnumerable<BaseUnitEntity> units)
	{
		BaseUnitEntity baseUnitEntity = null;
		foreach (BaseUnitEntity unit in units)
		{
			if (baseUnitEntity == null)
			{
				baseUnitEntity = unit;
			}
			else if (unit.DistanceToInCells(context.Caster) < baseUnitEntity.DistanceToInCells(context.Caster))
			{
				baseUnitEntity = unit;
			}
		}
		return baseUnitEntity;
	}

	private static BaseUnitEntity GetFarthestUnit(AbilityExecutionContext context, IEnumerable<BaseUnitEntity> units)
	{
		BaseUnitEntity baseUnitEntity = null;
		foreach (BaseUnitEntity unit in units)
		{
			if (baseUnitEntity == null)
			{
				baseUnitEntity = unit;
			}
			else if (unit.DistanceToInCells(context.Caster) > baseUnitEntity.DistanceToInCells(context.Caster))
			{
				baseUnitEntity = unit;
			}
		}
		return baseUnitEntity;
	}

	private static IEnumerable<TargetWrapper> ConvertToIEnumerable(BaseUnitEntity unit)
	{
		yield return new TargetWrapper(unit);
	}
}
