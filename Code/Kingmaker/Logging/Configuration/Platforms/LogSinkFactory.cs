using System;
using System.IO;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.QA;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CommandLineArgs;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.Logging.Configuration.Platforms;

public static class LogSinkFactory
{
	private const string ForceSkipKey = "-skipLogChannels";

	public static IDisposableLogSink CreateShort(string logFilePath, string filename, bool backup = false)
	{
		if (backup)
		{
			BackUpLog(filename, logFilePath);
		}
		UberLoggerFile logger = new UberLoggerFile(filename, logFilePath);
		string[] forceChannels = new string[1] { "MatchLight" };
		string[] skippedChannels = GetSkippedChannels();
		return new UberLoggerFilter(logger, LogSeverity.Warning, forceChannels, skippedChannels);
	}

	public static IDisposableLogSink CreateFull(string logFilePath, string filename, bool backup = false)
	{
		if (backup)
		{
			BackUpLog(filename, logFilePath);
		}
		return new UberLoggerFile(filename, logFilePath);
	}

	public static ILogSink CreateHistory()
	{
		return new GameHistoryLogSink(AreaDataStash.GameHistoryFile);
	}

	public static ILogSink CreateElementsDebugger()
	{
		return new ElementsDebugger.LogSink();
	}

	private static void BackUpLog(string filename, string path)
	{
		string text = Path.Combine(path, filename);
		if (File.Exists(text))
		{
			string path2 = Path.GetFileNameWithoutExtension(filename) + "Prev" + Path.GetExtension(filename);
			File.Copy(text, Path.Combine(path, path2), overwrite: true);
			File.Delete(text);
		}
	}

	private static string[] GetSkippedChannels()
	{
		string text;
		if (BuildModeUtility.Data.ForceSkipLogChannels != null)
		{
			text = BuildModeUtility.Data.ForceSkipLogChannels;
		}
		else
		{
			CommandLineArguments commandLineArguments = CommandLineArguments.Parse();
			text = (commandLineArguments.Contains("-skipLogChannels") ? commandLineArguments.Get("-skipLogChannels") : null);
		}
		return text?.Split(';') ?? Array.Empty<string>();
	}

	public static IDisposableLogSink AddSpamDetector()
	{
		return StackTraceSpamDetector.LogSink;
	}
}
