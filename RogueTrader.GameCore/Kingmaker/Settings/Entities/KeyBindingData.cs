using System;
using UnityEngine;

namespace Kingmaker.Settings.Entities;

[Serializable]
public struct KeyBindingData
{
	public KeyCode Key;

	public bool IsCtrlDown;

	public bool IsAltDown;

	public bool IsShiftDown;

	public bool IsPressed => Input.GetKeyDown(Key);

	public bool IsDown => Input.GetKey(Key);

	public KeyBindingData(string keyBind)
	{
		IsCtrlDown = keyBind.Contains("%");
		IsAltDown = keyBind.Contains("&");
		IsShiftDown = keyBind.Contains("#");
		if (!Enum.TryParse<KeyCode>(keyBind.TrimStart('%', '&', '#'), out Key))
		{
			throw new ArgumentException("[Settings] Can't parse binding '" + keyBind + "'");
		}
	}

	public bool IsIdenticalNotNone(KeyBindingData secondOne)
	{
		if (Key == KeyCode.None)
		{
			return false;
		}
		if (Key == secondOne.Key && IsCtrlDown == secondOne.IsCtrlDown && IsAltDown == secondOne.IsAltDown)
		{
			return IsShiftDown == secondOne.IsShiftDown;
		}
		return false;
	}

	public override string ToString()
	{
		string text = "";
		if (IsCtrlDown)
		{
			text += "%";
		}
		if (IsAltDown)
		{
			text += "&";
		}
		if (IsShiftDown)
		{
			text += "#";
		}
		return text + Key;
	}

	private bool Equals(KeyBindingData other)
	{
		if (Key == other.Key && IsCtrlDown == other.IsCtrlDown && IsAltDown == other.IsAltDown)
		{
			return IsShiftDown == other.IsShiftDown;
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
		return Equals((KeyBindingData)obj);
	}

	public override int GetHashCode()
	{
		return ((((((int)Key * 397) ^ IsCtrlDown.GetHashCode()) * 397) ^ IsAltDown.GetHashCode()) * 397) ^ IsShiftDown.GetHashCode();
	}
}
