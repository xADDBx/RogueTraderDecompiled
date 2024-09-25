using System;

namespace Kingmaker.Globalmap.Colonization;

public static class ColoniesStateExtensions
{
	public static bool TryGetColonyByGuid(this ColoniesState coloniesState, string guid, out Colony colony)
	{
		int i = 0;
		for (int count = coloniesState.Colonies.Count; i < count; i++)
		{
			Colony colony2 = coloniesState.Colonies[i].Colony;
			if (guid.Equals(colony2.Blueprint.AssetGuid, StringComparison.Ordinal))
			{
				colony = colony2;
				return true;
			}
		}
		colony = null;
		return false;
	}
}
