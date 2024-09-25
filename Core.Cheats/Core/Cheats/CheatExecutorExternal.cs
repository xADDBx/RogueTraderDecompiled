using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Cheats.Exceptions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Core.Cheats;

public class CheatExecutorExternal
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private readonly CheatDatabase m_CheatDatabase;

	private readonly Action<string, string> m_ExternalDelegate;

	public CheatExecutorExternal(CheatDatabase cheatDatabase, Action<string, string> externalDelegate)
	{
		m_CheatDatabase = cheatDatabase;
		m_ExternalDelegate = externalDelegate;
	}

	public Task ExecuteExternalWithDefaultLogging(string externalName, string externalNameWithArgs)
	{
		Logger.Log("Executing external {0}", externalNameWithArgs);
		try
		{
			object obj = ExecuteExternal(externalName, externalNameWithArgs);
			Logger.Log(obj?.ToString() ?? "Command ok");
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			return Task.FromException(ex);
		}
		return Task.CompletedTask;
	}

	private object ExecuteExternal(string externalName, string externalNameWithArgs)
	{
		if (!m_CheatDatabase.ExternalCommands.Contains(externalName))
		{
			throw new CommandNotFoundException(externalName);
		}
		bool isPlaying = Application.isPlaying;
		if (!ExecutionPolicy.PlayMode.IsAllowedNow(isPlaying))
		{
			throw new ExecutionPolicyException(externalName, ExecutionPolicy.PlayMode, isPlaying);
		}
		m_ExternalDelegate(externalName, externalNameWithArgs);
		return null;
	}
}
