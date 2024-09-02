using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules.Damage;

public static class CalculateDamageCache
{
	private static readonly Dictionary<CalculateDamageParams, RuleCalculateDamage> Cache = new Dictionary<CalculateDamageParams, RuleCalculateDamage>(100);

	private static int s_LastFrame;

	private static void InvalidateCacheIfNecessary()
	{
		int currentSystemStepIndex = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		if (s_LastFrame != currentSystemStepIndex)
		{
			s_LastFrame = currentSystemStepIndex;
			Cache.Clear();
		}
	}

	public static void ForceInvalidateCache()
	{
		s_LastFrame = 0;
		Cache.Clear();
	}

	[CanBeNull]
	public static RuleCalculateDamage Get(CalculateDamageParams @params)
	{
		InvalidateCacheIfNecessary();
		return Cache.Get(@params);
	}

	public static void Set(CalculateDamageParams @params, RuleCalculateDamage rule)
	{
		InvalidateCacheIfNecessary();
		Cache[@params] = rule;
	}
}
