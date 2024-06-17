using System;
using Kingmaker.Globalmap.Blueprints;

namespace Kingmaker.UI.Common;

public static class UIUtilityGetSpaceSystemExplored
{
	public static int GetPercentExplored(BlueprintStarSystemMap starSystem)
	{
		if (starSystem == null)
		{
			return 0;
		}
		return (int)Math.Ceiling(Game.Instance.StarSystemMapController.RecalculateResearchProgress(starSystem));
	}
}
