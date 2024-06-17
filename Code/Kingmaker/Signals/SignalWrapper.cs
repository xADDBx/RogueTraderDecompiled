using Newtonsoft.Json;

namespace Kingmaker.Signals;

public readonly struct SignalWrapper
{
	public static readonly SignalWrapper Empty;

	[JsonProperty]
	public readonly uint Key;

	public bool IsEmpty => Key == 0;

	public SignalWrapper(uint key)
	{
		Key = key;
	}

	public override string ToString()
	{
		if (!IsEmpty)
		{
			uint key = Key;
			return key.ToString();
		}
		return "Empty";
	}
}
