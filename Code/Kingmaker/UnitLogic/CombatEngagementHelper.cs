using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public static class CombatEngagementHelper
{
	private static readonly Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> Engage = new Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>>();

	private static readonly Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> EngagedBy = new Dictionary<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>>();

	private static int s_SystemStepIndex;

	public static void DropCacheIfNecessary()
	{
		if (s_SystemStepIndex == Game.Instance.RealTimeController.CurrentSystemStepIndex)
		{
			return;
		}
		foreach (KeyValuePair<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> item in Engage)
		{
			ReleaseList(item.Value);
		}
		Engage.Clear();
		foreach (KeyValuePair<EntityRef<BaseUnitEntity>, List<EntityRef<BaseUnitEntity>>> item2 in EngagedBy)
		{
			ReleaseList(item2.Value);
		}
		EngagedBy.Clear();
		s_SystemStepIndex = Time.frameCount;
	}

	public static IEnumerable<BaseUnitEntity> GetEngagedUnits(this BaseUnitEntity unit)
	{
		return Dereference(GetEngageListInternal(unit));
	}

	public static IEnumerable<BaseUnitEntity> GetEngagedByUnits(this BaseUnitEntity unit)
	{
		return Dereference(GetEngagedByListInternal(unit));
	}

	public static bool IsEngage(this BaseUnitEntity attacker, BaseUnitEntity target)
	{
		return GetEngageListInternal(attacker).Contains(target);
	}

	public static bool IsEngagedBy(this BaseUnitEntity target, BaseUnitEntity attacker)
	{
		return GetEngagedByListInternal(target).Contains(attacker);
	}

	private static List<EntityRef<BaseUnitEntity>> ClaimList()
	{
		return ListPool<EntityRef<BaseUnitEntity>>.Claim();
	}

	private static void ReleaseList(List<EntityRef<BaseUnitEntity>> list)
	{
		ListPool<EntityRef<BaseUnitEntity>>.Release(list);
	}

	private static List<EntityRef<BaseUnitEntity>> GetEngageListInternal(BaseUnitEntity unit)
	{
		DropCacheIfNecessary();
		return Engage.Get(unit) ?? CollectEngage(unit);
	}

	private static List<EntityRef<BaseUnitEntity>> GetEngagedByListInternal(BaseUnitEntity unit)
	{
		DropCacheIfNecessary();
		return EngagedBy.Get(unit) ?? CollectEngagedBy(unit);
	}

	private static IEnumerable<BaseUnitEntity> Dereference(List<EntityRef<BaseUnitEntity>> list)
	{
		foreach (EntityRef<BaseUnitEntity> item in list)
		{
			if (item.Entity != null)
			{
				yield return item.Entity;
			}
		}
	}

	public static List<EntityRef<BaseUnitEntity>> CollectUnitsAround(BaseUnitEntity unit, [CanBeNull] Func<BaseUnitEntity, bool> predicate = null)
	{
		List<EntityRef<BaseUnitEntity>> list = ClaimList();
		CustomGridNode currentUnwalkableNode = unit.CurrentUnwalkableNode;
		if (currentUnwalkableNode == null)
		{
			PFLog.Default.Warning($"Cannot collect engage list! Null origin node for unit {unit}");
			return list;
		}
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(currentUnwalkableNode, unit.SizeRect, 1))
		{
			BaseUnitEntity unit2 = item.GetUnit();
			if (unit2 != null && (predicate == null || predicate(unit2)))
			{
				list.Add(unit2);
			}
		}
		return list;
	}

	private static List<EntityRef<BaseUnitEntity>> CollectEngage(BaseUnitEntity unit)
	{
		return Engage[unit] = CollectUnitsAround(unit, (BaseUnitEntity entity) => unit.IsThreat(entity));
	}

	private static List<EntityRef<BaseUnitEntity>> CollectEngagedBy(BaseUnitEntity unit)
	{
		return EngagedBy[unit] = CollectUnitsAround(unit, (BaseUnitEntity entity) => entity.IsThreat(unit));
	}

	public static bool IsEngagedInPosition(this BaseUnitEntity unit, Vector3 desiredPosition)
	{
		CustomGridNodeBase nearestNodeXZUnwalkable = desiredPosition.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null)
		{
			PFLog.Default.Warning($"Null origin node for unit {unit}");
			return false;
		}
		foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNodeXZUnwalkable, unit.SizeRect, 1))
		{
			BaseUnitEntity unit2 = item.GetUnit();
			if (unit2 != null && unit2.IsThreat(unit))
			{
				return true;
			}
		}
		return false;
	}
}
