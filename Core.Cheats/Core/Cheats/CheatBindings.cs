using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Core.Cheats;

public static class CheatBindings
{
	private static readonly ConcurrentDictionary<string, string> _activeBindings = new ConcurrentDictionary<string, string>();

	public static string Version { get; private set; } = Guid.NewGuid().ToString();


	public static CheatBindingInfo[] ActiveBindings => _activeBindings.Select(delegate(KeyValuePair<string, string> v)
	{
		CheatBindingInfo result = default(CheatBindingInfo);
		result.Binding = v.Key;
		result.Command = v.Value;
		return result;
	}).ToArray();

	[Cheat(Name = "bind")]
	public static bool Register(string binding, string command)
	{
		string key = binding.ToLower();
		if (_activeBindings.TryAdd(key, command))
		{
			Version = Guid.NewGuid().ToString();
			return true;
		}
		return false;
	}

	[Cheat(Name = "unbind")]
	public static bool Unregister(string binding)
	{
		string key = binding.ToLower();
		if (_activeBindings.TryRemove(key, out var _))
		{
			Version = Guid.NewGuid().ToString();
			return true;
		}
		return false;
	}

	[Cheat(Name = "list_bindings")]
	public static string ListBindings()
	{
		return string.Join(Environment.NewLine, _activeBindings.Select((KeyValuePair<string, string> v) => v.Key + ": " + v.Value));
	}
}
