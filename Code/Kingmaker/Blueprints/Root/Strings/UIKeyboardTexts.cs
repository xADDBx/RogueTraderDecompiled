using System;
using System.Collections.Generic;
using System.Text;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIKeyboardTexts
{
	[Serializable]
	public class KeyCodeName
	{
		public KeyCode Code;

		public string CodeString;
	}

	[FormerlySerializedAs("LocalizedKeyCodes")]
	public List<KeyCodeName> KeyCodeNames;

	public static UIKeyboardTexts Instance => UIStrings.Instance.KeyboardTexts;

	public string GetStringByBinding(KeyboardAccess.Binding code)
	{
		if (code == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (code.IsCtrlDown)
		{
			stringBuilder.Append("Ctrl+");
		}
		if (code.IsAltDown)
		{
			stringBuilder.Append("Alt+");
		}
		if (code.IsShiftDown)
		{
			stringBuilder.Append("Shift+");
		}
		stringBuilder.Append(GetStringByKeyCode(code.Key));
		return stringBuilder.ToString();
	}

	public string GetStringByKeyCode(KeyCode kc)
	{
		if (kc == KeyCode.None)
		{
			return string.Empty;
		}
		return KeyCodeNames.FirstOrDefault((KeyCodeName p) => p.Code == kc)?.CodeString ?? kc.ToString();
	}
}
