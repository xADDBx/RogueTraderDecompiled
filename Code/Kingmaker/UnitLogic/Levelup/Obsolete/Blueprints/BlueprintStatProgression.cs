using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints;

[TypeId("cfd9ea8dce58fa24d8cd7ce89f9e0781")]
public class BlueprintStatProgression : BlueprintScriptableObject
{
	public int[] Bonuses;

	public int GetBonus(int level)
	{
		if (level < 0 || level >= Bonuses.Length)
		{
			PFLog.Default.Error("Can't find value for level {0} in {1}", level, this);
			return 0;
		}
		return Bonuses[level];
	}

	public int GetLevel(int bonus)
	{
		for (int i = 0; i < Bonuses.Length; i++)
		{
			if (bonus < Bonuses[i])
			{
				return Math.Max(0, i - 1);
			}
		}
		return Bonuses.Length - 1;
	}
}
