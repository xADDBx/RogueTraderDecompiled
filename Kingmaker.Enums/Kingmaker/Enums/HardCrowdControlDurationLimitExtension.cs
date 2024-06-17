using Kingmaker.Utility;

namespace Kingmaker.Enums;

public static class HardCrowdControlDurationLimitExtension
{
	public static Rounds ToRounds(this HardCrowdControlDurationLimit limit)
	{
		return ((int)limit).Rounds();
	}
}
