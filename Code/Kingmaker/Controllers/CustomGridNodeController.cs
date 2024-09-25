using System.Collections.Generic;
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
	private readonly Dictionary<CustomGridNodeBase, EntityRef<BaseUnitEntity>> m_Cache = new Dictionary<CustomGridNodeBase, EntityRef<BaseUnitEntity>>();

	public bool ContainsUnit(CustomGridNodeBase node)
	{
		return m_Cache.ContainsKey(node);
	}

	public bool ContainsUnit(CustomGridNodeBase node, BaseUnitEntity unit)
	{
		if (m_Cache.TryGetValue(node, out var value))
		{
			return value == unit;
		}
		return false;
	}

	public bool TryGetUnit(CustomGridNodeBase node, out BaseUnitEntity unit)
	{
		if (m_Cache.TryGetValue(node, out var value))
		{
			unit = value.Entity;
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
				BaseUnitEntity entity = m_Cache.Get(unitNode).Entity;
				if (entity == null)
				{
					m_Cache.Add(unitNode, allBaseAwakeUnit);
				}
				else if (isInCombat && allBaseAwakeUnit.CombatState.IsInCombat && !(Game.Instance.CurrentMode != GameModeType.Default))
				{
					if (!entity.CombatState.IsInCombat)
					{
						m_Cache[unitNode] = allBaseAwakeUnit;
					}
					else if (!CanOccupySameNode(allBaseAwakeUnit, entity))
					{
						PFLog.Default.Error("Two units occupy the same node: ({0}, {1}), {2}, {3}", unitNode.XCoordinateInGrid, unitNode.ZCoordinateInGrid, entity, allBaseAwakeUnit);
					}
				}
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
