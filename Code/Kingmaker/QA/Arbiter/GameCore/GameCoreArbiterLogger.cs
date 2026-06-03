using System;
using Kingmaker.QA.Arbiter.Service;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.QA.Arbiter.GameCore;

public class GameCoreArbiterLogger : IArbiterLogger
{
	private readonly LogChannel _channel;

	public GameCoreArbiterLogger(LogChannel channel)
	{
		_channel = channel;
	}

	public void Log(string messageFormat, params object[] @params)
	{
		_channel.Log(messageFormat, @params);
	}

	public void Warning(string messageFormat, params object[] @params)
	{
		_channel.Warning(messageFormat, @params);
	}

	public void Warning(Exception ex, string messageFormat, params object[] @params)
	{
		_channel.Warning(ex, messageFormat, @params);
	}

	public void Error(string messageFormat, params object[] @params)
	{
		_channel.Error(messageFormat, @params);
	}

	public void Error(Exception ex, string messageFormat, params object[] @params)
	{
		_channel.Error(ex, messageFormat, @params);
	}

	public void Exception(Exception ex, string messageFormat, params object[] @params)
	{
		_channel.Exception(ex, messageFormat, @params);
	}

	public void Exception(Exception ex)
	{
		_channel.Exception(ex);
	}
}
