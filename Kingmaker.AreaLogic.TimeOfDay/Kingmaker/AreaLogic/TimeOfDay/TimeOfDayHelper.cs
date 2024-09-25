using System;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.AreaLogic.TimeOfDay;

public static class TimeOfDayHelper
{
	public static TimeOfDay TimeOfDay(this TimeSpan gameTime)
	{
		int hours = gameTime.Hours;
		if (hours < 6)
		{
			return Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Night;
		}
		if (hours >= 6 && hours < 12)
		{
			return Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Morning;
		}
		if (hours >= 12 && hours < 18)
		{
			return Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Day;
		}
		return Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Evening;
	}

	public static TimeSpan Time(this TimeOfDay gameTime)
	{
		return gameTime switch
		{
			Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Morning => 6.Hours(), 
			Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Day => 12.Hours(), 
			Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Evening => 18.Hours(), 
			Kingmaker.AreaLogic.TimeOfDay.TimeOfDay.Night => 0.Hours(), 
			_ => throw new ArgumentOutOfRangeException("gameTime", gameTime, null), 
		};
	}
}
