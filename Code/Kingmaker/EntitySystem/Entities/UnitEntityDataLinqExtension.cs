using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.Combat;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public static class UnitEntityDataLinqExtension
{
	public static IEnumerable<PartUnitCombatState> CombatStates(this IEnumerable<BaseUnitEntity> source)
	{
		return source.Select((BaseUnitEntity u) => u.CombatState);
	}

	public static IEnumerable<BaseUnitEntity> Conscious(this IEnumerable<BaseUnitEntity> source)
	{
		return source.Where((BaseUnitEntity u) => u.LifeState.IsConscious);
	}

	public static IEnumerable<AbstractUnitEntity> NotDead(this IEnumerable<AbstractUnitEntity> source)
	{
		return source.Where((AbstractUnitEntity u) => !u.LifeState.IsDead);
	}

	public static IEnumerable<BaseUnitEntity> Dead(this IEnumerable<BaseUnitEntity> source)
	{
		return source.Where((BaseUnitEntity u) => u.LifeState.IsDead);
	}

	public static IEnumerable<BaseUnitEntity> InCombat(this IEnumerable<BaseUnitEntity> source)
	{
		return source.Where((BaseUnitEntity u) => u.IsInCombat);
	}

	public static IEnumerable<BaseUnitEntity> NotInCombat(this IEnumerable<BaseUnitEntity> source)
	{
		return source.Where((BaseUnitEntity u) => !u.IsInCombat);
	}

	public static IEnumerable<BaseUnitEntity> Npc(this IEnumerable<BaseUnitEntity> source)
	{
		return source.Where((BaseUnitEntity u) => !u.Faction.IsPlayer);
	}

	public static AbstractUnitEntity Nearest(this IEnumerable<AbstractUnitEntity> source, Vector3 point)
	{
		if (source != null && !source.Empty())
		{
			return source.Aggregate((AbstractUnitEntity nearest, AbstractUnitEntity unit) => (!(unit.DistanceTo(point) < nearest.DistanceTo(point))) ? nearest : unit);
		}
		return null;
	}

	public static BaseUnitEntity Nearest(this IEnumerable<BaseUnitEntity> source, Vector3 point)
	{
		if (source != null && !source.Empty())
		{
			return source.Aggregate((BaseUnitEntity nearest, BaseUnitEntity unit) => (!(unit.DistanceTo(point) < nearest.DistanceTo(point))) ? nearest : unit);
		}
		return null;
	}

	public static AbstractUnitEntity Nearest(this IEnumerable<AbstractUnitEntity> source, AbstractUnitEntity unit)
	{
		return source.Nearest(unit.Position);
	}

	public static BaseUnitEntity Nearest(this IEnumerable<BaseUnitEntity> source, BaseUnitEntity unit)
	{
		return source.Nearest(unit.Position);
	}
}
