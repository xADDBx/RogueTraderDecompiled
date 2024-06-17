using System;
using System.Collections.Generic;
using System.Linq;
using Core.Cheats;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CommandLineArgs;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kingmaker.QA;

public static class StackTraceSpamDetector
{
	private const string ReportMessageStackTrace = "Spam with stack trace rendered detected!";

	private const string ReportMessagePrompt = "Put a bug on this, because there will be lags on consolas!";

	private static TimeSpan s_FrameSize = TimeSpan.FromSeconds(1.0);

	private static int s_MaxAllowedExceptionsPerFrame = 5;

	private static DateTime[] s_Buffer = new DateTime[s_MaxAllowedExceptionsPerFrame];

	private static string[] s_Messages = new string[s_MaxAllowedExceptionsPerFrame];

	private static int s_BufferPointer = 0;

	private static TimeSpan s_CooldownTime = TimeSpan.FromSeconds(30.0);

	private static DateTime s_CooldownTimer = DateTime.MinValue;

	private static long s_SpamsDetectedWhileCooldown = 0L;

	private static bool s_MSuppressStackTraceDetection;

	private static bool IsDetectionEnabled
	{
		get
		{
			if ((!Application.isEditor || BuildModeUtility.ForceSpamDetectionInEditor) && !BuildModeUtility.StackTraceSpamDetectionDisabled && BuildModeUtility.IsDevelopment)
			{
				return s_FrameSize > TimeSpan.Zero;
			}
			return false;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		s_MSuppressStackTraceDetection = CommandLineArguments.Parse().Contains("-suppressStackTraceSpamDetection");
		if (!s_MSuppressStackTraceDetection)
		{
			Assert.raiseExceptions = false;
			Application.logMessageReceivedThreaded += LogCallback;
		}
	}

	[Cheat(Name = "spam_stacktrace_detection")]
	public static void StackTraceSpamDetectionEnable(int frameSize, int count = 5, int cooldown = 30000)
	{
		if (frameSize < 0)
		{
			throw new ArgumentOutOfRangeException("frameSize", "Should be greater or equals 0");
		}
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Should be greater than 0");
		}
		if (cooldown <= 0)
		{
			throw new ArgumentOutOfRangeException("cooldown", "Should be greater than 0");
		}
		s_FrameSize = TimeSpan.FromMilliseconds(frameSize);
		s_MaxAllowedExceptionsPerFrame = count;
		s_Buffer = new DateTime[s_MaxAllowedExceptionsPerFrame];
		s_Messages = new string[s_MaxAllowedExceptionsPerFrame];
		if (s_CooldownTimer > DateTime.MinValue)
		{
			s_CooldownTimer = s_CooldownTimer - s_CooldownTime + TimeSpan.FromMilliseconds(cooldown);
		}
		s_CooldownTime = TimeSpan.FromMilliseconds(cooldown);
	}

	private static void LogCallback(string condition, string stacktrace, LogType type)
	{
		if (!IsDetectionEnabled || !IsDetectionAllowed() || string.IsNullOrWhiteSpace(stacktrace))
		{
			return;
		}
		DateTime now = DateTime.Now;
		int num = s_BufferPointer++;
		if (s_BufferPointer >= s_MaxAllowedExceptionsPerFrame)
		{
			s_BufferPointer = 0;
		}
		DateTime dateTime = s_Buffer[num];
		s_Messages[num] = condition.Trim();
		s_Buffer[num] = now;
		if (now - dateTime > s_FrameSize)
		{
			return;
		}
		if (now > s_CooldownTimer)
		{
			DateTime cur = s_Buffer[num];
			List<string> times = (from tm in s_Buffer.Skip(num + 1).Concat(s_Buffer.Take(num + 1))
				select (tm - cur).ToString("ss'.'fff")).ToList();
			string text = string.Join("\n", s_Messages.Skip(num + 1).Concat(s_Messages.Take(num + 1)).Select((string msg, int i) => "[-" + times[i] + "] " + msg));
			string exceptionMessage = string.Format("{0} + {1} more times\n\n", "Spam with stack trace rendered detected!", s_SpamsDetectedWhileCooldown) + "[time]Last messages:\n\n" + text;
			QAModeExceptionEvents.Instance.MaybeShowError(string.Format("{0}\nGameModeType:{1}", "Put a bug on this, because there will be lags on consolas!", Game.Instance.CurrentMode), new SpamDetectingException(exceptionMessage, Game.Instance.CurrentMode));
			s_CooldownTimer = now + s_CooldownTime;
			s_SpamsDetectedWhileCooldown = 0L;
		}
		else
		{
			s_SpamsDetectedWhileCooldown++;
		}
	}

	private static bool IsDetectionAllowed()
	{
		try
		{
			if (LoadingProcess.Instance.IsLoadingInProcess)
			{
				return false;
			}
		}
		catch (Exception)
		{
			return false;
		}
		return true;
	}
}
