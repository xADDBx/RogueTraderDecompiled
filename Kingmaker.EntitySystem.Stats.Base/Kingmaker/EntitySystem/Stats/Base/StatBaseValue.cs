namespace Kingmaker.EntitySystem.Stats.Base;

public readonly struct StatBaseValue
{
	public readonly int Value;

	public readonly bool Enabled;

	private StatBaseValue(int value, bool enabled)
	{
		Value = value;
		Enabled = enabled;
	}

	public static implicit operator StatBaseValue((int Value, bool Enabled) v)
	{
		return new StatBaseValue(v.Value, v.Enabled);
	}

	public static implicit operator StatBaseValue(int value)
	{
		return new StatBaseValue(value, enabled: true);
	}
}
