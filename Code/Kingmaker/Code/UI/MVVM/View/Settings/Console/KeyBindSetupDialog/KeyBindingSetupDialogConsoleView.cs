using Kingmaker.Code.UI.MVVM.View.Settings.Base;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.InputSystems;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Settings.Console.KeyBindSetupDialog;

public class KeyBindingSetupDialogConsoleView : KeyBindingSetupDialogBaseView
{
	protected override bool GetValidBindingImpl(out KeyBindingData keyBindingData)
	{
		keyBindingData = new KeyBindingData
		{
			Key = KeyCode.None
		};
		if (!CommandKeyUp() || CommandKeyHold())
		{
			return false;
		}
		KeyCode key;
		if (KeyboardAccess.IsAltUp())
		{
			key = KeyCode.LeftAlt;
		}
		else if (KeyboardAccess.IsCtrlUp())
		{
			key = KeyCode.LeftControl;
		}
		else
		{
			if (!KeyboardAccess.IsShiftUp())
			{
				return false;
			}
			key = KeyCode.LeftShift;
		}
		keyBindingData.Key = key;
		return true;
	}
}
