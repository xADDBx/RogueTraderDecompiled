using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Cheats.Exceptions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Core.Cheats;

public class CheatExecutorCommand
{
	private static class TaskConverter<T>
	{
		public static async Task<object> WaitForTask(Task<T> task)
		{
			return await task;
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private readonly CheatDatabase m_Database;

	public CheatExecutorCommand(CheatDatabase database)
	{
		m_Database = database;
	}

	public async Task ExecuteCommandWithDefaultLogging(string command, string[] args)
	{
		Logger.Log("Executing command: {0} with args {1}", command, string.Join(", ", args));
		try
		{
			object obj = await ExecuteCommand(command, args);
			if (obj != null)
			{
				Logger.Log("Command result: {0}", obj);
			}
			else
			{
				Logger.Log("Command ok");
			}
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			throw;
		}
	}

	private async Task<object> ExecuteCommand(string command, string[] args)
	{
		if (!m_Database.CommandsByName.TryGetValue(command.ToLower(), out var value))
		{
			throw new CommandNotFoundException(command);
		}
		bool isPlaying = Application.isPlaying;
		if (!value.ExecutionPolicy.IsAllowedNow(isPlaying))
		{
			throw new ExecutionPolicyException(command, value.ExecutionPolicy, isPlaying);
		}
		return await ExecuteCmd(value, args);
	}

	private static async ValueTask<object> ExecuteCmd(CheatMethodInfo methodInfo, string[] args)
	{
		Delegate @delegate = (methodInfo as CheatMethodInfoInternal)?.Method;
		if ((object)@delegate == null)
		{
			throw new Exception("Cannot execute directly in external executor mode");
		}
		ParameterInfo[] parameters = @delegate.GetMethodInfo().GetParameters();
		int item = parameters.Select((ParameterInfo param, int i) => (param: param, i: i)).FirstOrDefault(((ParameterInfo param, int i) v) => v.param.HasDefaultValue).i;
		int num = parameters.Length;
		if (args.Length < item || args.Length > num)
		{
			throw new CommandArgumentCountException(args.Length, item, num);
		}
		object[] array = parameters.Zip(args, (ParameterInfo parameter, string argument) => ArgumentConverter.Convert(argument, parameter.ParameterType, parameter.Name)).Concat(Enumerable.Repeat(Type.Missing, num - args.Length)).ToArray();
		return await TryWait(@delegate.DynamicInvoke((array.Length == 0) ? null : array), @delegate.GetMethodInfo());
	}

	private static async ValueTask<object> TryWait(object obj, MethodInfo method)
	{
		if (obj == null)
		{
			return null;
		}
		if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
		{
			return await (Task<object>)typeof(TaskConverter<>).MakeGenericType(obj.GetType().GetGenericArguments().Single()).GetMethod("WaitForTask", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[1] { obj });
		}
		if (method.ReturnType == typeof(Task))
		{
			await (Task)obj;
			return null;
		}
		return obj;
	}
}
