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
using Kingmaker.Utility.Random;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[TypeId("786365e83e824d718b64d6a04cf5ee81")]
public class AbilityTargetsWithHighestHealthPoints : AbilitySelectTarget
{
	private enum IfEqualsOptions
	{
		Closest,
		Random,
		All
	}

	[SerializeField]
	private TargetType m_TargetType;

	[SerializeField]
	private bool m_IncludeCaster;

	[SerializeField]
	private ConditionsChecker m_Condition;

	[SerializeField]
	private IfEqualsOptions m_IfEqualsOptions;

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
			Enumerable.Empty<TargetWrapper>();
		}
		List<BaseUnitEntity> currentEquals;
		BaseUnitEntity highestHealthPoints = GetHighestHealthPoints(enumerable, out currentEquals);
		if (currentEquals.Empty())
		{
			return ConvertToIEnumerable(highestHealthPoints);
		}
		currentEquals.Add(highestHealthPoints);
		switch (m_IfEqualsOptions)
		{
		case IfEqualsOptions.Random:
		{
			int index = PFStatefulRandom.UnitLogic.Abilities.Range(0, currentEquals.Count);
			return ConvertToIEnumerable(currentEquals[index]);
		}
		case IfEqualsOptions.Closest:
			return ConvertToIEnumerable(GetClosestUnit(context, currentEquals));
		case IfEqualsOptions.All:
			return currentEquals.Select((BaseUnitEntity u) => new TargetWrapper(u));
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private static BaseUnitEntity GetHighestHealthPoints(IEnumerable<BaseUnitEntity> targets, out List<BaseUnitEntity> currentEquals)
	{
		BaseUnitEntity baseUnitEntity = null;
		currentEquals = TempList.Get<BaseUnitEntity>();
		foreach (BaseUnitEntity target in targets)
		{
			if (baseUnitEntity == null)
			{
				baseUnitEntity = target;
				continue;
			}
			if (target.Health.MaxHitPoints == baseUnitEntity.Health.MaxHitPoints)
			{
				currentEquals.Add(target);
			}
			if (target.Health.MaxHitPoints > baseUnitEntity.Health.MaxHitPoints)
			{
				currentEquals.Clear();
				baseUnitEntity = target;
			}
		}
		return baseUnitEntity;
	}

	private static BaseUnitEntity GetClosestUnit(AbilityExecutionContext context, List<BaseUnitEntity> currentEquals)
	{
		BaseUnitEntity baseUnitEntity = null;
		foreach (BaseUnitEntity currentEqual in currentEquals)
		{
			if (baseUnitEntity == null)
			{
				baseUnitEntity = currentEqual;
			}
			else if (currentEqual.DistanceToInCells(context.Caster) < baseUnitEntity.DistanceToInCells(context.Caster))
			{
				baseUnitEntity = currentEqual;
			}
		}
		return baseUnitEntity;
	}

	private static IEnumerable<TargetWrapper> ConvertToIEnumerable(BaseUnitEntity unit)
	{
		yield return new TargetWrapper(unit);
	}
}
