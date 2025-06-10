using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.AI;

public static class UnitSquadExtensions
{
	public static BaseUnitEntity SelectLeader(this UnitSquad squad)
	{
		BaseUnitEntity baseUnitEntity = squad.GetActingUnitsWithLeaderFirst().FirstOrDefault();
		if (baseUnitEntity != null)
		{
			return baseUnitEntity;
		}
		UnitReference unitReference = squad.Units.FirstOrDefault((UnitReference x) => !x.Entity.IsDeadOrUnconscious);
		if (unitReference != null)
		{
			baseUnitEntity = unitReference.ToBaseUnitEntity();
		}
		return baseUnitEntity;
	}

	public static IEnumerable<BaseUnitEntity> GetActingUnitsWithLeaderFirst(this UnitSquad squad)
	{
		BaseUnitEntity leader = squad.Leader;
		if (leader != null && leader.IsConsciousAndCanAct())
		{
			yield return squad.Leader;
		}
		IEnumerable<BaseUnitEntity> enumerable = from x in squad.Units
			where x.Entity != squad.Leader && x.Entity.IsConsciousAndCanAct()
			select x.Entity.ToBaseUnitEntity();
		foreach (BaseUnitEntity item in enumerable)
		{
			yield return item;
		}
	}

	private static bool IsConsciousAndCanAct(this IAbstractUnitEntity unit)
	{
		if (!unit.IsDeadOrUnconscious)
		{
			return unit.ToBaseUnitEntity().State.CanActInTurnBased;
		}
		return false;
	}
}
