using System;
using UnityEngine;

namespace Kingmaker.Utility.DotNetExtensions;

public static class TimeSpanExtension
{
	private const int HoursPerSegment = 8;

	private const int SegmentsPerYear = 1000;

	public static TimeSpan Seconds(this int value)
	{
		return ((double)value).Seconds();
	}

	public static TimeSpan Seconds(this float value)
	{
		return ((double)value).Seconds();
	}

	public static TimeSpan Seconds(this double value)
	{
		return Interval(value, 1000L, 10000000L);
	}

	public static TimeSpan Minutes(this int value)
	{
		return ((double)value).Minutes();
	}

	public static TimeSpan Minutes(this float value)
	{
		return ((double)value).Minutes();
	}

	public static TimeSpan Minutes(this double value)
	{
		return TimeSpan.FromMinutes(value);
	}

	public static TimeSpan Hours(this int value)
	{
		return ((double)value).Hours();
	}

	public static TimeSpan Hours(this float value)
	{
		return ((double)value).Hours();
	}

	public static TimeSpan Hours(this double value)
	{
		return TimeSpan.FromHours(value);
	}

	public static TimeSpan Days(this int value)
	{
		return ((double)value).Days();
	}

	public static TimeSpan Days(this float value)
	{
		return ((double)value).Days();
	}

	public static TimeSpan Days(this double value)
	{
		return TimeSpan.FromDays(value);
	}

	public static TimeSpan Weeks(this int value)
	{
		return ((double)value).Weeks();
	}

	public static TimeSpan Weeks(this float value)
	{
		return ((double)value).Weeks();
	}

	public static TimeSpan Weeks(this double value)
	{
		return (value * 7.0).Days();
	}

	public static TimeSpan Segments(this int value)
	{
		return ((double)value).Segments();
	}

	public static TimeSpan Segments(this float value)
	{
		return ((double)value).Segments();
	}

	public static TimeSpan Segments(this double value)
	{
		return (value * 8.0).Hours();
	}

	public static TimeSpan WarhammerYears(this int value)
	{
		return ((double)value).WarhammerYears();
	}

	public static TimeSpan WarhammerYears(this float value)
	{
		return ((double)value).WarhammerYears();
	}

	public static TimeSpan WarhammerYears(this double value)
	{
		return (value * 1000.0).Segments();
	}

	public static int TotalSegments(this TimeSpan value)
	{
		return (int)Math.Floor(value.TotalHours) / 8;
	}

	public static int Segments(this TimeSpan value)
	{
		return value.TotalSegments() % 1000;
	}

	public static int TotalWarhammerYears(this TimeSpan value)
	{
		return value.TotalSegments() / 1000;
	}

	public static long TotalMillisecondsInt(this TimeSpan value)
	{
		return (int)(value.Ticks / 10000);
	}

	public static long TotalMillisecondsLong(this TimeSpan value)
	{
		return value.Ticks / 10000;
	}

	public static TimeSpan Lerp(this TimeSpan a, TimeSpan b, float t)
	{
		return TimeSpan.FromTicks(LerpLong(a.Ticks, b.Ticks, t));
		static long LerpLong(long a, long b, float t)
		{
			return (long)((float)a + (float)(b - a) * Mathf.Clamp01(t));
		}
	}

	private static TimeSpan Interval(double value, long scaleToMilliseconds, long scaleToTicks)
	{
		if (double.IsNaN(value))
		{
			throw new ArgumentException("value is NaN");
		}
		double num = value * (double)scaleToMilliseconds + ((0.0 <= value) ? 0.5 : (-0.5));
		if (num < -922337203685477.0 || 922337203685477.0 < num)
		{
			throw new OverflowException($"TimeSpanTooLong {num}");
		}
		return new TimeSpan((long)(value * (double)scaleToTicks + ((0.0 <= value) ? 0.5 : (-0.5))));
	}
}
