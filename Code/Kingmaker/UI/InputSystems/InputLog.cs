using System;
using System.Collections;
using System.Text;
using Core.Cheats;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.UI.InputSystems;

public static class InputLog
{
	public static bool InputLogEnabled;

	public static void Log(string message)
	{
		if (InputLogEnabled)
		{
			PFLog.UI.Log(message);
		}
	}

	[Cheat(Name = "log_key_input", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static bool LogInput()
	{
		InputLogEnabled = !InputLogEnabled;
		return InputLogEnabled;
	}

	[Cheat(Name = "log_current_input_state", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void LogCurrentInput(float delay = 5f)
	{
		if (delay <= 0f)
		{
			LogKeyboardState();
		}
		else
		{
			CoroutineRunner.Start(LogWithDelay(delay));
		}
	}

	private static IEnumerator LogWithDelay(float delay = 5f)
	{
		yield return new WaitForSeconds(delay);
		LogKeyboardState();
	}

	private static void LogKeyboardState()
	{
		if (Game.Instance.UISettingsManager.IsNewKeyBindingSelectionHappening)
		{
			PFLog.UI.Log("New Key Binding Selection is Happening");
		}
		if (KeyboardAccess.IsInputFieldSelected())
		{
			PFLog.UI.Log("InputField Is Selected");
		}
		if ((bool)Game.Instance.Keyboard.Disabled)
		{
			PFLog.UI.Log($"Keyboard is Disabled. GuardCount = {Game.Instance.Keyboard.Disabled.GuardCount}");
		}
		Array values = Enum.GetValues(typeof(KeyCode));
		StringBuilder stringBuilder = new StringBuilder("Currently pressed keys:");
		foreach (object item in values)
		{
			if (Input.GetKey((KeyCode)item))
			{
				stringBuilder.Append(item);
				stringBuilder.Append(", ");
			}
		}
		PFLog.UI.Log(stringBuilder.ToString());
	}

	public static void SetLogInput(bool state)
	{
		InputLogEnabled = state;
	}
}
