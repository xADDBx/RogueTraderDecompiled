using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using Pathfinding;

namespace Kingmaker.Pathfinding;

public static class NodeTraverseCostHelper
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("NodeTraverseCostHelper");

	private static readonly List<OverrideCostData> OverrideCosts = new List<OverrideCostData>();

	public static Dictionary<GraphNode, float> GetOverrideCosts(AbstractUnitEntity entity)
	{
		Dictionary<GraphNode, float> dictionary = new Dictionary<GraphNode, float>();
		if (!entity.IsInCombat)
		{
			return dictionary;
		}
		if (!(entity is BaseUnitEntity unitEntity))
		{
			return dictionary;
		}
		foreach (OverrideCostData overrideCost in OverrideCosts)
		{
			if (!overrideCost.IsCorrectUnit(unitEntity))
			{
				continue;
			}
			foreach (CustomGridNodeBase node in overrideCost.Nodes)
			{
				dictionary[node] = (float)overrideCost.OverridePercentCost / 100f;
			}
		}
		return dictionary;
	}

	public static void AddOverrideCost(EntityFactSource source, int overridePercentCost, RestrictionsHolder.Reference restrictions)
	{
		if (OverrideCosts.Any((OverrideCostData x) => x.Source == source && x.OverridePercentCost == overridePercentCost))
		{
			Logger.Log($"Cost already overriden by {source}");
			return;
		}
		OverrideCostData overrideCostData = new OverrideCostData(source, overridePercentCost, restrictions);
		OverrideCosts.Add(overrideCostData);
		Logger.Log($"Add override cost {overrideCostData}");
	}

	public static void RemoveOverrideCost(EntityFactSource source)
	{
		if (OverrideCosts.RemoveAll((OverrideCostData x) => x.Source == source) > 0)
		{
			Logger.Log($"Remove override cost from {source}");
		}
		else
		{
			Logger.Log($"No cost overrides from {source}");
		}
	}
}
