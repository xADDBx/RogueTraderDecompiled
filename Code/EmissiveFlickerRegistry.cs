using System.Collections.Generic;

public static class EmissiveFlickerRegistry
{
	private static readonly Dictionary<string, List<EmissiveFlickerReceiver>> _registry = new Dictionary<string, List<EmissiveFlickerReceiver>>();

	public static void Register(string id, EmissiveFlickerReceiver receiver)
	{
		if (!_registry.ContainsKey(id))
		{
			_registry[id] = new List<EmissiveFlickerReceiver>();
		}
		if (!_registry[id].Contains(receiver))
		{
			_registry[id].Add(receiver);
		}
	}

	public static void Unregister(string id, EmissiveFlickerReceiver receiver)
	{
		if (_registry.ContainsKey(id))
		{
			_registry[id].Remove(receiver);
		}
	}

	public static List<EmissiveFlickerReceiver> Get(string id)
	{
		_registry.TryGetValue(id, out var value);
		return value;
	}
}
