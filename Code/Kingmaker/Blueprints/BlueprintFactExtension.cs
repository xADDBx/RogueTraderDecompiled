using Kingmaker.Blueprints.Facts;

namespace Kingmaker.Blueprints;

public static class BlueprintFactExtension
{
	public static bool CheckIsGameState(this BlueprintFact blueprintFact)
	{
		BlueprintComponent[] componentsArray = blueprintFact.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (!componentsArray[i].CheckIsGameState())
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsGameState(this BlueprintFact fact)
	{
		bool? isGameStateCache = fact.IsGameStateCache;
		if (!isGameStateCache.HasValue)
		{
			bool? flag = (fact.IsGameStateCache = fact.CheckIsGameState());
			return flag.Value;
		}
		return isGameStateCache.GetValueOrDefault();
	}
}
