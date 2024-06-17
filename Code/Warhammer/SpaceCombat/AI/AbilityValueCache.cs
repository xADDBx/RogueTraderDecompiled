using System.Collections.Generic;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;

namespace Warhammer.SpaceCombat.AI;

public class AbilityValueCache
{
	private struct CacheKey
	{
		public CustomGridNodeBase node;

		public int direction;

		public Ability ability;
	}

	private Dictionary<CacheKey, int> cache = new Dictionary<CacheKey, int>();

	private readonly AbilityValueCalculator valueCalculator;

	public AbilityValueCache(AbilityValueCalculator valueCalculator)
	{
		this.valueCalculator = valueCalculator;
	}

	public int GetValue(ShipPath.DirectionalPathNode pathNode, Ability ability)
	{
		CacheKey cacheKey = default(CacheKey);
		cacheKey.node = pathNode.node;
		cacheKey.direction = pathNode.direction;
		cacheKey.ability = ability;
		CacheKey key = cacheKey;
		if (!cache.TryGetValue(key, out var value))
		{
			value = valueCalculator.Calculate(pathNode, ability);
			cache.Add(key, value);
		}
		return value;
	}
}
