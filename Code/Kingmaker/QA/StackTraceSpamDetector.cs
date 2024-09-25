using System;
using Core.Async;
using Core.Cheats;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CommandLineArgs;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.QA;

public static class StackTraceSpamDetector
{
	private class StackTraceSpamDetectorDisposableLogSink : IDisposableLogSink, ILogSink, IDisposable, IOpenLoadingScreenHandler, ISubscriber, ICloseLoadingScreenHandler, IGameModeHandler
	{
		private bool m_IsLoadingWindowOpened;

		private GameModeType m_GameMode = GameModeType.None;

		public StackTraceSpamDetectorDisposableLogSink()
		{
			EventBus.Subscribe(this);
		}

		public void Log(LogInfo logInfo)
		{
			try
			{
				if (!m_IsLoadingWindowOpened && !(m_GameMode == GameModeType.None))
				{
					LogCallback(logInfo);
				}
			}
			catch (Exception)
			{
			}
		}

		public void Destroy()
		{
		}

		public void Dispose()
		{
			EventBus.Unsubscribe(this);
		}

		public void HandleCloseLoadingScreen()
		{
			m_IsLoadingWindowOpened = false;
			DateTime dateTime = DateTime.Now + TimeSpan.FromSeconds(5.0);
			s_CooldownTimer = ((dateTime > s_CooldownTimer) ? dateTime : s_CooldownTimer);
		}

		public void HandleOpenLoadingScreen()
		{
			m_IsLoadingWindowOpened = true;
		}

		public void OnGameModeStart(GameModeType gameMode)
		{
			m_GameMode = gameMode;
		}

		public void OnGameModeStop(GameModeType gameMode)
		{
		}
	}

	private static TimeSpan s_CooldownTime = TimeSpan.FromSeconds(30.0);

	private static DateTime s_CooldownTimer = DateTime.MinValue;

	private static bool s_SuppressStackTraceDetection;

	private static RegistrationService<LogItem> s_RegistrerService = new RegistrationService<LogItem>(300);

	private static readonly CountInTimeDetectionStrategy ExtremelyFastSpam = new CountInTimeDetectionStrategy("Extremely fast spam!", "critical", "current", 10, TimeSpan.FromMilliseconds(100.0));

	private static readonly LongTermSpamDetectionStrategy LongTermFastSpam = new LongTermSpamDetectionStrategy("Fast long term spam!", "critical", "current", 40, TimeSpan.FromSeconds(1.0), 2);

	private static readonly LongTermSpamDetectionStrategy LongTermNormalSpam = new LongTermSpamDetectionStrategy("Long term spam! Put a bug on this, because there will be lags on consolas!", "normal", "current", 10, TimeSpan.FromSeconds(1.0), 3);

	private static readonly ShiftedCountInTimeDetectionStrategy NormalSpam = new ShiftedCountInTimeDetectionStrategy("Put a bug on this, because there will be lags on consolas!", "normal", "afterrelease", 10, TimeSpan.FromSeconds(1.0), 1, TimeSpan.FromSeconds(1.0));

	private static Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>[] s_DetectionStrategies = new Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>[4]
	{
		new Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>(ExtremelyFastSpam, MaybeShowError),
		new Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>(LongTermFastSpam, MaybeShowError),
		new Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>(LongTermNormalSpam, MaybeShowError),
		new Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>(NormalSpam, MaybeShowError)
	};

	public static IDisposableLogSink LogSink { get; } = new StackTraceSpamDetectorDisposableLogSink();


	private static bool IsDetectionEnabled
	{
		get
		{
			if (!s_SuppressStackTraceDetection && UnitySyncContextHolder.IsInUnity && (!Application.isEditor || BuildModeUtility.ForceSpamDetectionInEditor) && Application.isPlaying && !BuildModeUtility.StackTraceSpamDetectionDisabled)
			{
				return BuildModeUtility.IsDevelopment;
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
		if (s_CooldownTimer > DateTime.MinValue)
		{
			s_CooldownTimer = s_CooldownTimer - s_CooldownTime + TimeSpan.FromMilliseconds(cooldown);
		}
		s_CooldownTime = TimeSpan.FromMilliseconds(cooldown);
	}

	[Cheat(Name = "debug_spam_cooldown")]
	public static void StackTraceSpamDetectionEnable(int cooldownMs = 30000)
	{
		if (cooldownMs <= 0)
		{
			s_CooldownTimer = DateTime.MaxValue;
			return;
		}
		s_CooldownTime = TimeSpan.FromMilliseconds(cooldownMs);
		s_CooldownTimer = s_CooldownTimer - s_CooldownTime + TimeSpan.FromMilliseconds(cooldownMs);
	}

	[Cheat(Name = "debug_spam_detection_fast")]
	public static void SpamDetectionConfigFast(int count, int intervalMs)
	{
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Should be greater than 0");
		}
		if (intervalMs <= 0)
		{
			throw new ArgumentOutOfRangeException("intervalMs", "Should be greater than 0");
		}
		ExtremelyFastSpam.Set(count, intervalMs);
	}

	[Cheat(Name = "debug_spam_detection_longterm_fast")]
	public static void SpamDetectionConfigLongTermFast(int count, int intervalMs, int times)
	{
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Should be greater than 0");
		}
		if (intervalMs <= 0)
		{
			throw new ArgumentOutOfRangeException("intervalMs", "Should be greater than 0");
		}
		if (times < 1)
		{
			throw new ArgumentOutOfRangeException("intervalMs", "Should be greater than 0");
		}
		LongTermFastSpam.Set(count, intervalMs, times);
	}

	[Cheat(Name = "debug_spam_detection_longterm_normal")]
	public static void SpamDetectionConfigLongTermNormal(int count, int intervalMs, int times)
	{
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Should be greater than 0");
		}
		if (intervalMs <= 0)
		{
			throw new ArgumentOutOfRangeException("intervalMs", "Should be greater than 0");
		}
		if (times < 1)
		{
			throw new ArgumentOutOfRangeException("intervalMs", "Should be greater than 0");
		}
		LongTermNormalSpam.Set(count, intervalMs, times);
	}

	[Cheat(Name = "debug_spam_detection_normal")]
	public static void SpamDetectionConfigLongTermNormal(int count, int intervalMs)
	{
		if (count <= 0)
		{
			throw new ArgumentOutOfRangeException("count", "Should be greater than 0");
		}
		if (intervalMs <= 0)
		{
			throw new ArgumentOutOfRangeException("intervalMs", "Should be greater than 0");
		}
		NormalSpam.Set(count, intervalMs);
	}

	private static void LogCallback(LogInfo logInfo)
	{
		LogSeverity severity = logInfo.Severity;
		if ((severity != LogSeverity.Error && severity != LogSeverity.Warning) || !IsDetectionEnabled || logInfo.Callstack == null || logInfo.Callstack.Empty())
		{
			return;
		}
		DateTime now = DateTime.Now;
		if (now < s_CooldownTimer)
		{
			return;
		}
		LogItem logItem = new LogItem();
		logItem.Callstack = logInfo.Callstack[0].GetFormattedMethodName();
		logItem.Message = $"[{logInfo.GetTimeStampAsString()} - {logInfo.Channel?.Name}][{logInfo.Severity:G}]: {logInfo.Message}";
		logItem.Time = logInfo.TimeStamp;
		LogItem item = logItem;
		s_RegistrerService.Register(item);
		Tuple<ISpamDetectionStrategy, Action<SpamDetectionResult>>[] array = s_DetectionStrategies;
		for (int i = 0; i < array.Length; i++)
		{
			var (spamDetectionStrategy2, action2) = array[i];
			var (flag, spamDetectionResult) = spamDetectionStrategy2.Check(s_RegistrerService);
			if (spamDetectionResult != null)
			{
				action2(spamDetectionResult);
				s_RegistrerService.Clear();
				s_CooldownTimer = now + s_CooldownTime;
				break;
			}
			if (flag)
			{
				break;
			}
		}
	}

	private static void MaybeShowError(SpamDetectionResult result)
	{
		string message = result.Message;
		QAModeExceptionEvents.Instance.MaybeShowError($"{message}\nGameModeType:{Game.Instance.CurrentMode}\non [{DateTime.Now}]", new SpamDetectionException(Game.Instance.CurrentMode, result.Items));
	}
}
