using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Utility.CommandLineArgs;

public class CommandLineArguments
{
	private static CommandLineArguments s_Args;

	private readonly Dictionary<string, string> m_Arguments = new Dictionary<string, string>();

	public static CommandLineArguments Parse()
	{
		return s_Args ?? (s_Args = Parse(Environment.GetCommandLineArgs()));
	}

	public static CommandLineArguments Parse(string[] args)
	{
		CommandLineArguments commandLineArguments = new CommandLineArguments();
		for (int i = 0; i < args.Length; i++)
		{
			string[] array = args[i].Trim().Split('=');
			if (commandLineArguments.m_Arguments.ContainsKey(array[0]))
			{
				Debug.LogError("During parsing of command line arguments, a duplicate was found: " + array[0]);
			}
			else
			{
				commandLineArguments.m_Arguments.Add(array[0], (array.Length > 1) ? array[1] : string.Empty);
			}
		}
		return commandLineArguments;
	}

	public bool Contains(string key)
	{
		return m_Arguments.ContainsKey(key);
	}

	public bool Equals(string key, string value)
	{
		if (m_Arguments.ContainsKey(key))
		{
			return m_Arguments[key].Equals(value);
		}
		return false;
	}

	public string Get(string key)
	{
		if (!m_Arguments.TryGetValue(key, out var value))
		{
			return string.Empty;
		}
		return value;
	}

	public string Get(string key, string defaultValue)
	{
		string text = Get(key);
		if (!string.IsNullOrEmpty(text))
		{
			return text;
		}
		return defaultValue;
	}
}
