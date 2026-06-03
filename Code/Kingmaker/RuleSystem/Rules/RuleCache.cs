using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.RuleSystem.Rules;

public abstract class RuleCache<TParams, TRule>
{
	private static readonly Dictionary<TParams, TRule> Cache = new Dictionary<TParams, TRule>(100);

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
	public static TRule Get(TParams @params)
	{
		InvalidateCacheIfNecessary();
		return Cache.Get(@params);
	}

	public static void Set(TParams @params, TRule rule)
	{
		InvalidateCacheIfNecessary();
		Cache[@params] = rule;
	}
}
