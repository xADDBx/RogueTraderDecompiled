using System;
using System.Linq;
using System.Text;
using Core.Cheats;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.QA;

public static class StackTraceSpamDetector
{
	private class StackTraceSpamDetectorDisposableLogSink : IDisposableLogSink, ILogSink, IDisposable
	{
		public void Log(LogInfo logInfo)
		{
			LogCallback(logInfo);
		}

		public void Destroy()
		{
		}

		public void Dispose()
		{
		}
	}

	private const string ReportMessagePrompt = "Put a bug on this, because there will be lags on consolas!";

	private static TimeSpan s_FrameSize = TimeSpan.FromSeconds(1.0);

	private static int s_MaxAllowedExceptionsPerFrame = 5;

	private static DateTime[] s_Buffer = new DateTime[s_MaxAllowedExceptionsPerFrame];

	private static string[] s_Messages = new string[s_MaxAllowedExceptionsPerFrame];

	private static int s_BufferPointer = 0;

	private static TimeSpan s_CooldownTime = TimeSpan.FromSeconds(30.0);

	private static DateTime s_CooldownTimer = DateTime.MinValue;

	private static bool s_SuppressStackTraceDetection;

	public static IDisposableLogSink LogSink { get; } = new StackTraceSpamDetectorDisposableLogSink();


	private static bool IsDetectionEnabled
	{
		get
		{
			if ((!Application.isEditor || BuildModeUtility.ForceSpamDetectionInEditor) && Application.isPlaying && !BuildModeUtility.StackTraceSpamDetectionDisabled && BuildModeUtility.IsDevelopment)
			{
				return s_FrameSize > TimeSpan.Zero;
			}
			return false;
		}
	}

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	public static void Init()
	{
		s_SuppressStackTraceDetection = CommandLineArguments.Parse().Contains("-suppressStackTraceSpamDetection");
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

	private static void LogCallback(LogInfo logInfo)
	{
		LogSeverity severity = logInfo.Severity;
		if ((severity != LogSeverity.Error && severity != LogSeverity.Warning) || !IsDetectionAllowed() || logInfo.Callstack == null || logInfo.Callstack.Empty())
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
		s_Messages[num] = $"[{logInfo.GetTimeStampAsString()} - {logInfo.Channel?.Name}][{logInfo.Severity:G}]: {logInfo.Message}";
		s_Buffer[num] = now;
		if (now - dateTime > s_FrameSize || !(now > s_CooldownTimer))
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder("Logging calls that triggered a Spam exception\n");
		foreach (int item in Enumerable.Range(s_BufferPointer, s_MaxAllowedExceptionsPerFrame - s_BufferPointer).Concat(Enumerable.Range(0, s_BufferPointer)))
		{
			stringBuilder.AppendLine(s_Messages[item]);
		}
		QAModeExceptionEvents.Instance.MaybeShowError(string.Format("{0}\nGameModeType:{1}", "Put a bug on this, because there will be lags on consolas!", Game.Instance.CurrentMode), new SpamDetectingException(stringBuilder.ToString(), Game.Instance.CurrentMode));
		s_CooldownTimer = now + s_CooldownTime;
	}

	private static bool IsDetectionAllowed()
	{
		if (!IsDetectionEnabled)
		{
			return false;
		}
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
