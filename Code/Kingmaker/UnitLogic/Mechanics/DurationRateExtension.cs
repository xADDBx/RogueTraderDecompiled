using System;
using Kingmaker.Utility;

namespace Kingmaker.UnitLogic.Mechanics;

public static class DurationRateExtension
{
	public static DurationRate ToDurationRate(this Rounds rounds)
	{
		if (rounds < DurationRate.Minutes.ToRounds())
		{
			return DurationRate.Rounds;
		}
		if (rounds < DurationRate.TenMinutes.ToRounds())
		{
			return DurationRate.Minutes;
		}
		if (rounds < DurationRate.Hours.ToRounds())
		{
			return DurationRate.TenMinutes;
		}
		if (rounds < DurationRate.Days.ToRounds())
		{
			return DurationRate.Hours;
		}
		return DurationRate.Days;
	}

	public static Rounds ToRounds(this DurationRate rate)
	{
		return rate switch
		{
			DurationRate.Rounds => 1.Rounds(), 
			DurationRate.Minutes => 10.Rounds(), 
			DurationRate.TenMinutes => 100.Rounds(), 
			DurationRate.Hours => 600.Rounds(), 
			DurationRate.Days => 14400.Rounds(), 
			_ => throw new ArgumentOutOfRangeException("rate", rate, null), 
		};
	}
}
