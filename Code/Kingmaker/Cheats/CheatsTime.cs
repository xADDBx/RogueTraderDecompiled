using System;
using System.Globalization;
using Core.Cheats;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Cheats;

internal class CheatsTime
{
	private static int s_Grade = 3;

	private static readonly float[] s_Grades = new float[7] { 0.125f, 0.25f, 0.5f, 1f, 2f, 4f, 8f };

	private static long s_GameLastCall;

	private static long s_RealLastCall;

	public static void RegisterCommands(KeyboardAccess keyboard)
	{
		if (BuildModeUtility.CheatsEnabled)
		{
			keyboard.Bind("TimeScaleUp", delegate
			{
				CheatsHelper.Run("time_scale_up");
			});
			keyboard.Bind("TimeScaleDown", delegate
			{
				CheatsHelper.Run("time_scale_down");
			});
			keyboard.Bind("SkipDay", delegate
			{
				CheatsHelper.Run("skip_hours 24");
			});
			keyboard.Bind("SkipWeek", delegate
			{
				CheatsHelper.Run("skip_hours 168");
			});
		}
	}

	[Cheat(Name = "skip_hours", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SkipHours(int hours)
	{
		Game.Instance.AdvanceGameTime(hours.Hours());
		Game.Instance.MatchTimeOfDay();
	}

	[Cheat(Name = "reroll_weather", Description = "Force weather change", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void RerollWeather()
	{
		Game.Instance.Player.Weather.NextWeatherChange = Game.Instance.Player.GameTime;
	}

	[Cheat(Name = "time_scale_up", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void TimeScaleUp()
	{
		s_Grade = Math.Min(s_Grades.Length - 1, s_Grade + 1);
		string text = s_Grades[s_Grade].ToString(CultureInfo.InvariantCulture);
		CheatsRelease.TimeScale("time_scale " + text);
		UIUtility.SendWarning("Time scale " + text);
	}

	[Cheat(Name = "time_scale_down", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void TimeScaleDown()
	{
		s_Grade = Math.Max(0, s_Grade - 1);
		string text = s_Grades[s_Grade].ToString(CultureInfo.InvariantCulture);
		CheatsRelease.TimeScale("time_scale " + text);
		UIUtility.SendWarning("Time scale " + text);
	}

	[Cheat(Name = "get_time", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static string GetTime()
	{
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		TimeSpan realTime = Game.Instance.TimeController.RealTime;
		TimeSpan timeSpan = gameTime;
		string text = "Game time: " + timeSpan.ToString() + ((s_GameLastCall == 0L) ? "" : (". Delta: " + (gameTime.Ticks - s_GameLastCall) + " ms"));
		timeSpan = realTime;
		string text2 = "Real time: " + timeSpan.ToString() + ((s_RealLastCall == 0L) ? "" : (". Delta: " + (realTime.Ticks - s_RealLastCall) + " ms"));
		s_GameLastCall = gameTime.Ticks;
		s_RealLastCall = realTime.Ticks;
		return text + Environment.NewLine + text2;
	}
}
