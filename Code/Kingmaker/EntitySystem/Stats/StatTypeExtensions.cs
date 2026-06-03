using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.EntitySystem.Stats;

public static class StatTypeExtensions
{
	public static StatType TryGetOverride(this StatType type, MechanicEntity owner)
	{
		IReadOnlyDictionary<StatType, List<PartStatsContainer.StatOverrideData>> overridenStats = owner.GetRequired<PartStatsContainer>().OverridenStats;
		if (overridenStats != null && overridenStats.ContainsKey(type))
		{
			foreach (PartStatsContainer.StatOverrideData item in overridenStats[type])
			{
				if (item.CanBeUsed(owner))
				{
					return item.OverrideType;
				}
			}
		}
		return type;
	}
}
