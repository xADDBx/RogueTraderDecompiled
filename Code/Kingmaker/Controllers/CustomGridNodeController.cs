using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.Pathfinding;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers;

public class CustomGridNodeController : IControllerTick, IController, IControllerReset
{
	private readonly Dictionary<CustomGridNodeBase, List<EntityRef<BaseUnitEntity>>> m_Cache = new Dictionary<CustomGridNodeBase, List<EntityRef<BaseUnitEntity>>>();

	public bool ContainsUnit(CustomGridNodeBase node)
	{
		return m_Cache.ContainsKey(node);
	}

	public bool ContainsUnit(CustomGridNodeBase node, BaseUnitEntity unit)
	{
		if (m_Cache.TryGetValue(node, out var value))
		{
			return value.Contains(unit);
		}
		return false;
	}

	public bool TryGetUnit(CustomGridNodeBase node, out BaseUnitEntity unit)
	{
		if (m_Cache.TryGetValue(node, out var value))
		{
			unit = value.FirstOrDefault().Entity;
			return unit != null;
		}
		unit = null;
		return false;
	}

	[CanBeNull]
	public BaseUnitEntity GetUnit(CustomGridNodeBase node)
	{
		TryGetUnit(node, out var unit);
		return unit;
	}

	public BaseUnitEntity[] GetAllUnits(CustomGridNodeBase node)
	{
		if (m_Cache.TryGetValue(node, out var value))
		{
			return value.Select((EntityRef<BaseUnitEntity> uRef) => uRef.Entity).ToArray();
		}
		return null;
	}

	public void Clear()
	{
		m_Cache.Clear();
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		m_Cache.Clear();
		bool isInCombat = Game.Instance.Player.IsInCombat;
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			foreach (CustomGridNodeBase unitNode in GetUnitNodes(allBaseAwakeUnit))
			{
				if (!m_Cache.TryGetValue(unitNode, out var value))
				{
					Dictionary<CustomGridNodeBase, List<EntityRef<BaseUnitEntity>>> cache = m_Cache;
					List<EntityRef<BaseUnitEntity>> obj = new List<EntityRef<BaseUnitEntity>> { allBaseAwakeUnit };
					value = obj;
					cache.Add(unitNode, obj);
				}
				if (value.Contains(allBaseAwakeUnit))
				{
					continue;
				}
				if (value.Count > 0)
				{
					BaseUnitEntity entity = value[0].Entity;
					if (entity != null && !entity.IsInCombat && allBaseAwakeUnit.IsInCombat)
					{
						value.Insert(0, allBaseAwakeUnit);
						continue;
					}
				}
				value.Add(allBaseAwakeUnit);
			}
		}
		if (!isInCombat || Game.Instance.CurrentMode != GameModeType.Default)
		{
			return;
		}
		foreach (CustomGridNodeBase key in m_Cache.Keys)
		{
			List<EntityRef<BaseUnitEntity>> list = m_Cache.Get(key);
			if (list.Count != 1 && list.Count(delegate(EntityRef<BaseUnitEntity> u)
			{
				BaseUnitEntity entity2 = u.Entity;
				if (entity2 != null && entity2.IsInCombat)
				{
					PartStarshipNavigation starshipNavigationOptional = u.Entity.GetStarshipNavigationOptional();
					if (starshipNavigationOptional == null)
					{
						return false;
					}
					return !starshipNavigationOptional.IsSoftUnit;
				}
				return false;
			}) > 1)
			{
				PFLog.Default.Error("Two units occupy the same node: ({0}, {1}) : {2}", key.XCoordinateInGrid, key.ZCoordinateInGrid, list);
			}
		}
	}

	private static bool CanOccupySameNode(BaseUnitEntity unit1, BaseUnitEntity unit2)
	{
		PartStarshipNavigation starshipNavigationOptional = unit1.GetStarshipNavigationOptional();
		if (starshipNavigationOptional == null || !starshipNavigationOptional.IsSoftUnit)
		{
			return unit2.GetStarshipNavigationOptional()?.IsSoftUnit ?? false;
		}
		return true;
	}

	public static NodeList GetUnitNodes(BaseUnitEntity unit)
	{
		if (!unit.CombatState.IsInCombat)
		{
			return unit.GetOccupiedNodes();
		}
		return unit.MovementAgent.Blocker.BlockedNodes;
	}

	void IControllerReset.OnReset()
	{
		Clear();
	}
}
