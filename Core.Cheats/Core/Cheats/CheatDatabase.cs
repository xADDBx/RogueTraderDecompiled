using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Reflection;
using Owlcat.Runtime.Core.Logging;

namespace Core.Cheats;

public class CheatDatabase
{
	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private readonly Task<(IReadOnlyDictionary<string, CheatMethodInfo> commands, IReadOnlyDictionary<string, CheatPropertyInfo> variables)> m_InternalsTask;

	private Task<string[]> m_ExternalCommandsTask = Task.FromResult(Array.Empty<string>());

	public string Version { get; private set; }

	public IReadOnlyDictionary<string, CheatMethodInfo> CommandsByName => m_InternalsTask.Result.commands;

	public IReadOnlyDictionary<string, CheatPropertyInfo> PropertiesByName => m_InternalsTask.Result.variables;

	public string[] ExternalCommands => m_ExternalCommandsTask.Result;

	private static Dictionary<string, CheatMethodInfo> CollectMethods()
	{
		Dictionary<string, CheatMethodInfo> dictionary = new Dictionary<string, CheatMethodInfo>();
		Assembly[] assembliesSafe = AppDomain.CurrentDomain.GetAssembliesSafe();
		foreach (Assembly assembly in assembliesSafe)
		{
			Type type = assembly.GetType("CheatsCodeGen.AllCheats");
			if (type == null)
			{
				continue;
			}
			FieldInfo field = type.GetField("Methods");
			if (field == null)
			{
				Logger.Error("Assemble " + assembly.FullName + " contains CheatsCodeGen, but it doesn't have field Methods!");
				continue;
			}
			foreach (CheatMethodInfoInternal item in (List<CheatMethodInfoInternal>)field.GetValue(null))
			{
				if (dictionary.ContainsKey(item.Name))
				{
					Logger.Error("Cheat method name collision: {0} in assembly {1}, from method: {2}", item.Name, assembly.FullName, item.Method.Method.ToString());
				}
				else
				{
					dictionary.Add(item.Name, item);
				}
			}
		}
		return dictionary;
	}

	private static Dictionary<string, CheatPropertyInfo> CollectProperties()
	{
		Dictionary<string, CheatPropertyInfo> dictionary = new Dictionary<string, CheatPropertyInfo>();
		Assembly[] assembliesSafe = AppDomain.CurrentDomain.GetAssembliesSafe();
		foreach (Assembly assembly in assembliesSafe)
		{
			Type type = assembly.GetType("CheatsCodeGen.AllCheats");
			if (type == null)
			{
				continue;
			}
			FieldInfo field = type.GetField("Properties");
			if (field == null)
			{
				Logger.Error("Assemble " + assembly.FullName + " contains CheatsCodeGen, but it doesn't have field Properties!");
				continue;
			}
			foreach (CheatPropertyInfoInternal item in (List<CheatPropertyInfoInternal>)field.GetValue(null))
			{
				if (dictionary.ContainsKey(item.Name))
				{
					Logger.Error("Cheat property name collision: {0} in assembly {1}", item.Name, assembly.FullName);
				}
				else
				{
					dictionary.Add(item.Name, item);
				}
			}
		}
		return dictionary;
	}

	private static (IReadOnlyDictionary<string, CheatMethodInfo> commands, IReadOnlyDictionary<string, CheatPropertyInfo> variables) GetInternals()
	{
		return (commands: CollectMethods(), variables: CollectProperties());
	}

	public CheatDatabase()
	{
		Version = Guid.NewGuid().ToString();
		m_InternalsTask = Task.Run((Func<(IReadOnlyDictionary<string, CheatMethodInfo>, IReadOnlyDictionary<string, CheatPropertyInfo>)>)GetInternals);
	}

	public CheatDatabase(in KnownObjectsInfo knownObjects)
	{
		Version = knownObjects.Version;
		Dictionary<string, CheatPropertyInfo> item = knownObjects.Variables.ToDictionary((CheatPropertyInfo v) => v.Name);
		Dictionary<string, CheatMethodInfo> item2 = knownObjects.Methods.ToDictionary((CheatMethodInfo v) => v.Name);
		m_InternalsTask = Task.FromResult(((IReadOnlyDictionary<string, CheatMethodInfo>)item2, (IReadOnlyDictionary<string, CheatPropertyInfo>)item));
		m_ExternalCommandsTask = Task.FromResult(knownObjects.Externals.ToArray());
	}

	public void SetExternals(IEnumerable<string> externals)
	{
		m_ExternalCommandsTask = Task.Run(() => CollectDuplicates(externals));
		Version = Guid.NewGuid().ToString();
	}

	private async Task<string[]> CollectDuplicates(IEnumerable<string> externals)
	{
		IReadOnlyDictionary<string, CheatMethodInfo> internalCommands = (await m_InternalsTask).Item1;
		IReadOnlyDictionary<string, CheatPropertyInfo> item = (await m_InternalsTask).Item2;
		IEnumerable<(string, string)> source = from command in externals
			let lower = command.ToLower()
			join known in internalCommands.Keys.Concat(item.Keys) on lower equals known into duplicates
			from duplicate in duplicates.DefaultIfEmpty()
			select (command: lower, duplicate: duplicate);
		foreach (var item2 in source.Where<(string, string)>(((string command, string duplicate) v) => v.duplicate != null))
		{
			Logger.Error("External name {0} is already taken by {1}", item2.Item1, item2.Item2);
		}
		return (from v in source
			where v.duplicate == null
			select v.command).ToArray();
	}

	public KnownObjectsInfo GetKnownObjects()
	{
		KnownObjectsInfo result = default(KnownObjectsInfo);
		result.Version = Version;
		result.Methods = CommandsByName.Values.ToArray();
		result.Variables = PropertiesByName.Values.ToArray();
		result.Externals = ExternalCommands;
		return result;
	}
}
