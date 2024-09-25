using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Core.Cheats.Exceptions;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Core.Cheats;

public class CheatsParser
{
	public delegate Task ExecuteExternalDelegate(string name, string nameWithArgs);

	public delegate Task ExecuteCommandDelegate(string command, string[] args);

	public delegate Task ExecuteGetVariableDelegate(string variable);

	public delegate Task ExecuteSetVariableDelegate(string variable, string value);

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Console");

	private readonly CheatDatabase m_Database;

	private readonly ExecuteExternalDelegate m_ExternalDelegate;

	private readonly ExecuteCommandDelegate m_CommandDelegate;

	private readonly ExecuteGetVariableDelegate m_GetVarDelegate;

	private readonly ExecuteSetVariableDelegate m_SetVarDelegate;

	private static readonly Regex CommandSplitterRegex = new Regex("\"(.+?)\"|(\\S+)");

	private static readonly Regex VariableSetSplitterRegex = new Regex("^(?'VarName'\\w+)\\s*(=|)\\s*(\"(?'VarVal1'.+?)\"|(?'VarVal2'\\S+))$");

	public CheatsParser(CheatDatabase database, ExecuteExternalDelegate externalDelegate, ExecuteCommandDelegate commandDelegate, ExecuteGetVariableDelegate getVarDelegate, ExecuteSetVariableDelegate setVarDelegate)
	{
		m_Database = database;
		m_ExternalDelegate = externalDelegate;
		m_CommandDelegate = commandDelegate;
		m_GetVarDelegate = getVarDelegate;
		m_SetVarDelegate = setVarDelegate;
	}

	public IEnumerable<string> TryAutocomplete(string pieceOfCommand)
	{
		return TryAutocomplete(pieceOfCommand, Application.isPlaying);
	}

	public IEnumerable<string> TryAutocomplete(string pieceOfCommand, bool isPlaying)
	{
		if (string.IsNullOrEmpty(pieceOfCommand))
		{
			return Enumerable.Empty<string>();
		}
		string lowerPiece = pieceOfCommand.Trim().ToLower();
		if (string.IsNullOrEmpty(lowerPiece))
		{
			return Enumerable.Empty<string>();
		}
		IEnumerable<string> first = from v in m_Database.CommandsByName
			where IsCommandOk(v.Key, v.Value.ExecutionPolicy)
			select v.Key;
		IEnumerable<string> second = m_Database.ExternalCommands.Where((string v) => IsCommandOk(v, ExecutionPolicy.PlayMode));
		return from v in Enumerable.Concat(second: from v in m_Database.PropertiesByName
				where IsCommandOk(v.Key, v.Value.ExecutionPolicy)
				select v.Key, first: first.Concat(second)).Take(20)
			orderby v.IndexOf(pieceOfCommand, StringComparison.InvariantCultureIgnoreCase), v
			select v;
		bool IsCommandOk(string name, ExecutionPolicy executionPolicy)
		{
			if (name.Contains(lowerPiece))
			{
				return executionPolicy.IsAllowedNow(isPlaying);
			}
			return false;
		}
	}

	private (CheatMethodInfo command, string methodName, string[] args) ParseCommand(string command)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return (command: null, methodName: "", args: Array.Empty<string>());
		}
		MatchCollection matchCollection = CommandSplitterRegex.Matches(command);
		if (matchCollection.Count == 0)
		{
			return (command: null, methodName: "", args: Array.Empty<string>());
		}
		string text = (string.IsNullOrWhiteSpace(matchCollection[0].Groups[1].Value) ? matchCollection[0].Groups[2].Value : matchCollection[0].Groups[1].Value);
		string[] item = (from v in matchCollection.Skip(1)
			select (!string.IsNullOrEmpty(v.Groups[1].Value)) ? v.Groups[1].Value : v.Groups[2].Value).ToArray();
		m_Database.CommandsByName.TryGetValue(text.ToLower(), out var value);
		return (command: value, methodName: text.ToLower(), args: item);
	}

	private (CheatPropertyInfo variable, string varName, string value) ParseVariableSet(string command)
	{
		if (string.IsNullOrWhiteSpace(command))
		{
			return (variable: null, varName: "", value: "");
		}
		MatchCollection matchCollection = VariableSetSplitterRegex.Matches(command);
		if (matchCollection.Count == 0)
		{
			return (variable: null, varName: "", value: "");
		}
		string text = matchCollection[0].Groups["VarName"].Value.ToLower();
		string item = (string.IsNullOrWhiteSpace(matchCollection[0].Groups["VarVal1"].Value) ? matchCollection[0].Groups["VarVal2"].Value : matchCollection[0].Groups["VarVal1"].Value);
		m_Database.PropertiesByName.TryGetValue(text, out var value);
		return (variable: value, varName: text, value: item);
	}

	public Task Execute(string command)
	{
		var (cheatMethodInfo, text, array) = ParseCommand(command);
		var (cheatPropertyInfo, value, parameter2) = ParseVariableSet(command);
		if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(value))
		{
			throw new CommandParseException(command);
		}
		if (cheatPropertyInfo != null)
		{
			string value2 = ArgumentConverter.Preprocess(parameter2, cheatPropertyInfo.Type, cheatPropertyInfo.Name);
			return m_SetVarDelegate(cheatPropertyInfo.Name, value2);
		}
		if (cheatMethodInfo != null)
		{
			string[] args = array.Zip(cheatMethodInfo.Parameters, (string argument, CheatParameter parameter) => ArgumentConverter.Preprocess(argument, parameter.Type, parameter.Name)).ToArray();
			return m_CommandDelegate(cheatMethodInfo.Name, args);
		}
		if (m_Database.ExternalCommands.Contains(text))
		{
			return m_ExternalDelegate(text, command);
		}
		if (!string.IsNullOrWhiteSpace(text) && array.Length == 0 && m_Database.PropertiesByName.TryGetValue(text, out var _))
		{
			return m_GetVarDelegate(text);
		}
		try
		{
			throw new CommandNotFoundException(text);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex);
			throw;
		}
	}
}
