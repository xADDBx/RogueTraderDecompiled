using System;

namespace Kingmaker.Utility;

public static class RoundsExtension
{
	public static Rounds Rounds(this int value)
	{
		return new Rounds(value);
	}

	public static Rounds ToRounds(this TimeSpan timeSpan)
	{
		return new Rounds(Math.Max(0, (int)(timeSpan.TotalSeconds / 5.0)));
	}
}
