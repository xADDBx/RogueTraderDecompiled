using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Kingmaker.Settings.ConstructionHelpers.KeyPrefix;

public class SettingsKeyPrefix : IDisposable
{
	private static Stack<SettingsKeyPrefix> s_Prefixes = new Stack<SettingsKeyPrefix>();

	private readonly string m_LocalPrefix;

	public static string Prefix { get; private set; } = "";


	public static bool HasPrefix => s_Prefixes.Count > 0;

	public SettingsKeyPrefix(string localPrefix)
	{
		if (!Regex.IsMatch(localPrefix, "^[0-9a-z-]+$"))
		{
			throw new Exception("[Settings] Key prefixes can contain only letter and dashes (-)");
		}
		m_LocalPrefix = localPrefix;
		Prefix = Prefix + m_LocalPrefix + ".";
		s_Prefixes.Push(this);
	}

	public void Dispose()
	{
		if (s_Prefixes.Peek() != this)
		{
			throw new Exception("[Settings] Key prefixes are disposed in wrong order");
		}
		s_Prefixes.Pop();
		Prefix = "";
		foreach (SettingsKeyPrefix s_Prefix in s_Prefixes)
		{
			Prefix = s_Prefix.m_LocalPrefix + "." + Prefix;
		}
	}
}
