using System.Text;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.KeyBindSetupDialog;

public static class KeyBindingDataExtensions
{
	public static string GetPrettyString(this KeyBindingData keyBindingData)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (keyBindingData.IsCtrlDown)
		{
			stringBuilder.Append("Ctrl+");
		}
		if (keyBindingData.IsAltDown)
		{
			stringBuilder.Append("Alt+");
		}
		if (keyBindingData.IsShiftDown)
		{
			stringBuilder.Append("Shift+");
		}
		if (keyBindingData.Key != 0)
		{
			KeyCode[] altCodes = KeyboardAccess.AltCodes;
			for (int i = 0; i < altCodes.Length; i++)
			{
				if (altCodes[i] == keyBindingData.Key)
				{
					return "Alt";
				}
			}
			altCodes = KeyboardAccess.CtrlCodes;
			for (int i = 0; i < altCodes.Length; i++)
			{
				if (altCodes[i] == keyBindingData.Key)
				{
					return "Ctrl";
				}
			}
			altCodes = KeyboardAccess.ShiftCodes;
			for (int i = 0; i < altCodes.Length; i++)
			{
				if (altCodes[i] == keyBindingData.Key)
				{
					return "Shift";
				}
			}
			stringBuilder.Append(GetKeyCodeString(keyBindingData.Key));
		}
		return stringBuilder.ToString();
	}

	private static string GetKeyCodeString(KeyCode key)
	{
		KeyCode[] altCodes = KeyboardAccess.AltCodes;
		for (int i = 0; i < altCodes.Length; i++)
		{
			if (altCodes[i] == key)
			{
				return "Alt";
			}
		}
		altCodes = KeyboardAccess.CtrlCodes;
		for (int i = 0; i < altCodes.Length; i++)
		{
			if (altCodes[i] == key)
			{
				return "Ctrl";
			}
		}
		altCodes = KeyboardAccess.ShiftCodes;
		for (int i = 0; i < altCodes.Length; i++)
		{
			if (altCodes[i] == key)
			{
				return "Shift";
			}
		}
		return UIStrings.Instance.KeyboardTexts.GetStringByKeyCode(key);
	}
}
