using System;
using Kingmaker.UI.InputSystems.Enums;

namespace Kingmaker.Settings.Entities;

[Serializable]
public struct KeyBindingPair
{
	public KeyBindingData Binding1;

	public KeyBindingData Binding2;

	public GameModesGroup GameModesGroup;

	public bool TriggerOnHold;

	public static bool IsKeyBindingString(string s)
	{
		return s.StartsWith("!");
	}

	public KeyBindingPair(string keyBindPair)
	{
		string[] array = keyBindPair.TrimStart('!').Split(';');
		string.IsNullOrEmpty(array[0]);
		Binding1 = (string.IsNullOrEmpty(array[0]) ? default(KeyBindingData) : new KeyBindingData(array[0]));
		Binding2 = (string.IsNullOrEmpty(array[1]) ? default(KeyBindingData) : new KeyBindingData(array[1]));
		if (!Enum.TryParse<GameModesGroup>(array[2], out GameModesGroup))
		{
			throw new ArgumentException("[Settings] Can't parse game mode group '" + array[2] + "'");
		}
		if (!bool.TryParse(array[3], out TriggerOnHold))
		{
			throw new ArgumentException("[Settings] Can't parse trigger on hold '" + array[3] + "'");
		}
	}

	public override string ToString()
	{
		return $"!{Binding1};{Binding2};{GameModesGroup};{TriggerOnHold}";
	}

	private bool Equals(KeyBindingPair other)
	{
		if (object.Equals(Binding1, other.Binding1) && object.Equals(Binding2, other.Binding2) && GameModesGroup == other.GameModesGroup)
		{
			return TriggerOnHold == other.TriggerOnHold;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if ((object)this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((KeyBindingPair)obj);
	}

	public override int GetHashCode()
	{
		return (int)(((uint)(((Binding1.GetHashCode() * 397) ^ Binding2.GetHashCode()) * 397) ^ (uint)GameModesGroup) * 397) ^ TriggerOnHold.GetHashCode();
	}
}
