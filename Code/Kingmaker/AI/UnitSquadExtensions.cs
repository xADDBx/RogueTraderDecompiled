using System.Collections.Generic;
using System.Linq;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Squads;

namespace Kingmaker.AI;

public static class UnitSquadExtensions
{
	public static BaseUnitEntity SelectLeader(this UnitSquad squad)
	{
		BaseUnitEntity leader = squad.Leader;
		BaseUnitEntity baseUnitEntity = ((leader != null && !leader.IsDeadOrUnconscious) ? squad.Leader : squad.GetConsciousUnits().FirstOrDefault());
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

	public static IEnumerable<BaseUnitEntity> GetConsciousUnits(this UnitSquad squad)
	{
		return from x in squad.Units
			where !x.Entity.IsDeadOrUnconscious && x.Entity.ToBaseUnitEntity().State.CanActInTurnBased
			select x.Entity.ToBaseUnitEntity();
	}
}
