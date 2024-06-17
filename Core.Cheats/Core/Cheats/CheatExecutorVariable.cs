using System;
using System.Threading.Tasks;
using Core.Cheats.Exceptions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Core.Cheats;

public class CheatExecutorVariable
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private readonly CheatDatabase m_Database;

	public CheatExecutorVariable(CheatDatabase database)
	{
		m_Database = database;
	}

	public Task ExecuteGetVariableWithDefaultLogging(string variable)
	{
		Logger.Log("Executing get variable {0}", variable);
		try
		{
			object obj = ExecuteGetVariable(variable);
			Logger.Log("Variable {0} value is {1}", variable, obj?.ToString() ?? "null");
			return Task.CompletedTask;
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			return Task.FromException(ex);
		}
	}

	private object ExecuteGetVariable(string variable)
	{
		if (!m_Database.PropertiesByName.TryGetValue(variable.ToLower(), out var value))
		{
			throw new CommandNotFoundException(variable);
		}
		bool isPlaying = Application.isPlaying;
		if (!value.ExecutionPolicy.IsAllowedNow(isPlaying))
		{
			throw new ExecutionPolicyException(variable, value.ExecutionPolicy, isPlaying);
		}
		return GetVariable(value);
	}

	private static object GetVariable(CheatPropertyInfo variable)
	{
		return ((variable as CheatPropertyInfoInternal)?.Methods.Getter ?? throw new Exception("Cannot execute directly in external executor mode")).DynamicInvoke();
	}

	public Task ExecuteSetVariableWithDefaultLogging(string variable, string value)
	{
		Logger.Log("Executing set variable {0} to value {1}", variable, value);
		try
		{
			ExecuteSetVariable(variable, value);
			Logger.Log("Variable set");
			return Task.CompletedTask;
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			return Task.FromException(ex);
		}
	}

	private void ExecuteSetVariable(string variable, string value)
	{
		if (!m_Database.PropertiesByName.TryGetValue(variable.ToLower(), out var value2))
		{
			throw new CommandNotFoundException(variable);
		}
		bool isPlaying = Application.isPlaying;
		if (!value2.ExecutionPolicy.IsAllowedNow(isPlaying))
		{
			throw new ExecutionPolicyException(variable, value2.ExecutionPolicy, isPlaying);
		}
		SetVariable(value2, value);
	}

	private static void SetVariable(CheatPropertyInfo variable, string value)
	{
		Delegate @delegate = (variable as CheatPropertyInfoInternal)?.Methods.Setter;
		if ((object)@delegate == null)
		{
			throw new Exception("Cannot execute directly in external executor mode");
		}
		object obj = ArgumentConverter.Convert(value, @delegate.Method.GetParameters()[0].ParameterType, variable.Name);
		@delegate.DynamicInvoke(obj);
	}
}
