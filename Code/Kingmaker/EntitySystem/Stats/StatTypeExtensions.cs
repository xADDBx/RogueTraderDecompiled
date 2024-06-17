using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Stats;

public static class StatTypeExtensions
{
	public static StatType TryGetOverride(this StatType type, MechanicEntity owner)
	{
		Dictionary<StatType, StatType> overridenStats = owner.GetRequired<PartStatsContainer>().OverridenStats;
		if (overridenStats != null && overridenStats.ContainsKey(type))
		{
			return overridenStats[type];
		}
		return type;
	}
}
