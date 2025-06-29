using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker;
using UnityEngine;

namespace UnityModManagerNet;

[Serializable]
public class KeyBinding
{
	public KeyCode keyCode = (KeyCode)0;

	public byte modifiers;

	private int m_Index = -1;

	internal static object KeyControlZero;

	private object m_KeyControl;

	private static Type inputControlType;

	private static Type keyboardType;

	private static Type keyControlType;

	private static PropertyInfo currentKeyboardPI;

	private static PropertyInfo keyControlFromStringPI;

	private static PropertyInfo isPressedPI;

	private static PropertyInfo wasPressedThisFramePI;

	private static PropertyInfo wasReleasedThisFramePI;

	private static object keyboard;

	private static object keyControlCtrl;

	private static object keyControlShift;

	private static object keyControlAlt;

	internal static object keyControlEscape;

	internal static string[] KeysCode;

	internal static string[] KeysName;

	private static bool hasErrors;

	private static readonly Dictionary<string, string> EnabledKeys;

	private static readonly Dictionary<string, string> LegacyToInputSystemMap;

	internal int Index
	{
		get
		{
			if (m_Index == -1)
			{
				m_Index = Array.FindIndex(KeysCode, (string x) => x == ((object)(KeyCode)(ref keyCode)).ToString());
			}
			if (m_Index == -1)
			{
				Change((KeyCode)0, 0);
				return 0;
			}
			return m_Index;
		}
	}

	internal object KeyControl
	{
		get
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Invalid comparison between Unknown and I4
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Invalid comparison between Unknown and I4
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			if (!LegacyInputDisabled || keyboard == null)
			{
				return KeyControlZero;
			}
			if (m_KeyControl == null && (int)keyCode > 0 && LegacyToInputSystemMap.TryGetValue(((object)(KeyCode)(ref keyCode)).ToString(), out var value))
			{
				m_KeyControl = keyControlFromStringPI.GetValue(keyboard, new object[1] { value });
			}
			if (m_KeyControl == null)
			{
				m_KeyControl = KeyControlZero;
				if ((int)keyCode > 0)
				{
					PFLog.UnityModManager.Log($"Key {keyCode} was not found in new Input System.");
				}
			}
			return m_KeyControl;
		}
	}

	public static bool LegacyInputDisabled { get; private set; }

	static KeyBinding()
	{
		KeyControlZero = new object();
		hasErrors = true;
		EnabledKeys = new Dictionary<string, string>
		{
			{ "None", "None" },
			{ "BackQuote", "~" },
			{ "Tab", "Tab" },
			{ "Space", "Space" },
			{ "Return", "Enter" },
			{ "Alpha0", "0" },
			{ "Alpha1", "1" },
			{ "Alpha2", "2" },
			{ "Alpha3", "3" },
			{ "Alpha4", "4" },
			{ "Alpha5", "5" },
			{ "Alpha6", "6" },
			{ "Alpha7", "7" },
			{ "Alpha8", "8" },
			{ "Alpha9", "9" },
			{ "Minus", "-" },
			{ "Equals", "=" },
			{ "Backspace", "Backspace" },
			{ "F1", "F1" },
			{ "F2", "F2" },
			{ "F3", "F3" },
			{ "F4", "F4" },
			{ "F5", "F5" },
			{ "F6", "F6" },
			{ "F7", "F7" },
			{ "F8", "F8" },
			{ "F9", "F9" },
			{ "F10", "F10" },
			{ "F11", "F11" },
			{ "F12", "F12" },
			{ "A", "A" },
			{ "B", "B" },
			{ "C", "C" },
			{ "D", "D" },
			{ "E", "E" },
			{ "F", "F" },
			{ "G", "G" },
			{ "H", "H" },
			{ "I", "I" },
			{ "J", "J" },
			{ "K", "K" },
			{ "L", "L" },
			{ "M", "M" },
			{ "N", "N" },
			{ "O", "O" },
			{ "P", "P" },
			{ "Q", "Q" },
			{ "R", "R" },
			{ "S", "S" },
			{ "T", "T" },
			{ "U", "U" },
			{ "V", "V" },
			{ "W", "W" },
			{ "X", "X" },
			{ "Y", "Y" },
			{ "Z", "Z" },
			{ "LeftBracket", "[" },
			{ "RightBracket", "]" },
			{ "Semicolon", ";" },
			{ "Quote", "'" },
			{ "Backslash", "\\" },
			{ "Comma", "," },
			{ "Period", "." },
			{ "Slash", "/" },
			{ "Insert", "Insert" },
			{ "Home", "Home" },
			{ "Delete", "Delete" },
			{ "End", "End" },
			{ "PageUp", "Page Up" },
			{ "PageDown", "Page Down" },
			{ "UpArrow", "Up Arrow" },
			{ "DownArrow", "Down Arrow" },
			{ "RightArrow", "Right Arrow" },
			{ "LeftArrow", "Left Arrow" },
			{ "KeypadDivide", "Numpad /" },
			{ "KeypadMultiply", "Numpad *" },
			{ "KeypadMinus", "Numpad -" },
			{ "KeypadPlus", "Numpad +" },
			{ "KeypadEnter", "Numpad Enter" },
			{ "KeypadPeriod", "Numpad Del" },
			{ "Keypad0", "Numpad 0" },
			{ "Keypad1", "Numpad 1" },
			{ "Keypad2", "Numpad 2" },
			{ "Keypad3", "Numpad 3" },
			{ "Keypad4", "Numpad 4" },
			{ "Keypad5", "Numpad 5" },
			{ "Keypad6", "Numpad 6" },
			{ "Keypad7", "Numpad 7" },
			{ "Keypad8", "Numpad 8" },
			{ "Keypad9", "Numpad 9" },
			{ "RightShift", "Right Shift" },
			{ "LeftShift", "Left Shift" },
			{ "RightControl", "Right Ctrl" },
			{ "LeftControl", "Left Ctrl" },
			{ "RightAlt", "Right Alt" },
			{ "LeftAlt", "Left Alt" },
			{ "Pause", "Pause" },
			{ "Escape", "Escape" },
			{ "Numlock", "Num Lock" },
			{ "CapsLock", "Caps Lock" },
			{ "ScrollLock", "Scroll Lock" },
			{ "Print", "Print Screen" }
		};
		LegacyToInputSystemMap = new Dictionary<string, string>
		{
			{ "Backspace", "backspace" },
			{ "Tab", "tab" },
			{ "Pause", "pause" },
			{ "Escape", "escape" },
			{ "Space", "space" },
			{ "Return", "enter" },
			{ "Quote", "quote" },
			{ "Comma", "comma" },
			{ "Minus", "minus" },
			{ "Period", "period" },
			{ "Slash", "slash" },
			{ "Alpha0", "0" },
			{ "Alpha1", "1" },
			{ "Alpha2", "2" },
			{ "Alpha3", "3" },
			{ "Alpha4", "4" },
			{ "Alpha5", "5" },
			{ "Alpha6", "6" },
			{ "Alpha7", "7" },
			{ "Alpha8", "8" },
			{ "Alpha9", "9" },
			{ "Semicolon", "semicolon" },
			{ "Equals", "equals" },
			{ "LeftBracket", "leftBracket" },
			{ "Backslash", "backslash" },
			{ "RightBracket", "rightBracket" },
			{ "BackQuote", "backquote" },
			{ "A", "a" },
			{ "B", "b" },
			{ "C", "c" },
			{ "D", "d" },
			{ "E", "e" },
			{ "F", "f" },
			{ "G", "g" },
			{ "H", "h" },
			{ "I", "i" },
			{ "J", "j" },
			{ "K", "k" },
			{ "L", "l" },
			{ "M", "m" },
			{ "N", "n" },
			{ "O", "o" },
			{ "P", "p" },
			{ "Q", "q" },
			{ "R", "r" },
			{ "S", "s" },
			{ "T", "t" },
			{ "U", "u" },
			{ "V", "v" },
			{ "W", "w" },
			{ "X", "x" },
			{ "Y", "y" },
			{ "Z", "z" },
			{ "Keypad0", "numpad0" },
			{ "Keypad1", "numpad1" },
			{ "Keypad2", "numpad2" },
			{ "Keypad3", "numpad3" },
			{ "Keypad4", "numpad4" },
			{ "Keypad5", "numpad5" },
			{ "Keypad6", "numpad6" },
			{ "Keypad7", "numpad7" },
			{ "Keypad8", "numpad8" },
			{ "Keypad9", "numpad9" },
			{ "KeypadPeriod", "numpadPeriod" },
			{ "KeypadDivide", "numpadDivide" },
			{ "KeypadMultiply", "numpadMultiply" },
			{ "KeypadMinus", "numpadMinus" },
			{ "KeypadPlus", "numpadPlus" },
			{ "KeypadEnter", "numpadEnter" },
			{ "UpArrow", "upArrow" },
			{ "DownArrow", "downArrow" },
			{ "RightArrow", "rightArrow" },
			{ "LeftArrow", "leftArrow" },
			{ "Insert", "insert" },
			{ "Home", "home" },
			{ "End", "end" },
			{ "Delete", "delete" },
			{ "PageUp", "pageUp" },
			{ "PageDown", "pageDown" },
			{ "F1", "f1" },
			{ "F2", "f2" },
			{ "F3", "f3" },
			{ "F4", "f4" },
			{ "F5", "f5" },
			{ "F6", "f6" },
			{ "F7", "f7" },
			{ "F8", "f8" },
			{ "F9", "f9" },
			{ "F10", "f10" },
			{ "F11", "f11" },
			{ "F12", "f12" },
			{ "Numlock", "numLock" },
			{ "CapsLock", "capsLock" },
			{ "ScrollLock", "scrollLock" },
			{ "RightShift", "rightShift" },
			{ "LeftShift", "leftShift" },
			{ "RightControl", "rightCtrl" },
			{ "LeftControl", "leftCtrl" },
			{ "RightAlt", "rightAlt" },
			{ "LeftAlt", "leftAlt" },
			{ "Print", "printScreen" }
		};
		IEnumerable<string> source = EnabledKeys.Keys.Intersect(Enum.GetNames(typeof(KeyCode)));
		KeysCode = source.ToArray();
		KeysName = source.Select((string x) => EnabledKeys[x]).ToArray();
	}

	internal static void Initialize()
	{
		try
		{
			if (Input.GetKey((KeyCode)32))
			{
				LegacyInputDisabled = false;
			}
		}
		catch (Exception)
		{
			LegacyInputDisabled = true;
			PFLog.UnityModManager.Log("Legacy Input is disabled.");
		}
		if (!LegacyInputDisabled)
		{
			return;
		}
		try
		{
			Assembly assembly = Assembly.Load("Unity.InputSystem");
			inputControlType = assembly.GetType("UnityEngine.InputSystem.InputControl");
			if (inputControlType == null)
			{
				PFLog.UnityModManager.Error("Type UnityEngine.InputSystem.InputControl not found.");
			}
			keyboardType = assembly.GetType("UnityEngine.InputSystem.Keyboard");
			if (keyboardType == null)
			{
				PFLog.UnityModManager.Error("Type UnityEngine.InputSystem.Keyboard not found.");
			}
			keyControlType = assembly.GetType("UnityEngine.InputSystem.Controls.KeyControl");
			if (keyControlType == null)
			{
				PFLog.UnityModManager.Error("Type UnityEngine.InputSystem.Controls.KeyControl not found.");
			}
			currentKeyboardPI = keyboardType.GetProperty("current", BindingFlags.Static | BindingFlags.Public);
			if (currentKeyboardPI == null)
			{
				PFLog.UnityModManager.Error("Property current not found.");
			}
			keyControlFromStringPI = inputControlType.GetProperty("Item");
			if (keyControlFromStringPI == null)
			{
				PFLog.UnityModManager.Error("Property Item not found.");
			}
			isPressedPI = keyControlType.GetProperty("isPressed");
			if (isPressedPI == null)
			{
				PFLog.UnityModManager.Error("Property isPressed not found.");
			}
			wasPressedThisFramePI = keyControlType.GetProperty("wasPressedThisFrame");
			if (wasPressedThisFramePI == null)
			{
				PFLog.UnityModManager.Error("Property wasPressedThisFrame not found.");
			}
			wasReleasedThisFramePI = keyControlType.GetProperty("wasReleasedThisFrame");
			if (wasReleasedThisFramePI == null)
			{
				PFLog.UnityModManager.Error("Property wasReleasedThisFrame not found.");
			}
			hasErrors = currentKeyboardPI == null || keyControlFromStringPI == null || isPressedPI == null || wasPressedThisFramePI == null || wasReleasedThisFramePI == null;
		}
		catch (Exception ex2)
		{
			hasErrors = true;
			PFLog.UnityModManager.Exception(ex2, (string)null);
			PFLog.UnityModManager.Error("Legacy Input was marked as disabled, but new Input System was not found.");
		}
	}

	internal static void BindKeyboard()
	{
		if (!LegacyInputDisabled || hasErrors)
		{
			return;
		}
		object value = currentKeyboardPI.GetValue(null, null);
		if (value == keyboard)
		{
			return;
		}
		keyboard = value;
		if (keyboard != null)
		{
			PFLog.UnityModManager.Log("Detected keyboard.");
			keyControlCtrl = keyControlFromStringPI.GetValue(keyboard, new object[1] { "ctrl" });
			if (keyControlCtrl == null)
			{
				PFLog.UnityModManager.Error("Value keyControlCtrl is null.");
			}
			keyControlShift = keyControlFromStringPI.GetValue(keyboard, new object[1] { "shift" });
			if (keyControlShift == null)
			{
				PFLog.UnityModManager.Error("Value keyControlShift is null.");
			}
			keyControlAlt = keyControlFromStringPI.GetValue(keyboard, new object[1] { "alt" });
			if (keyControlAlt == null)
			{
				PFLog.UnityModManager.Error("Value keyControlAlt is null.");
			}
			keyControlEscape = keyControlFromStringPI.GetValue(keyboard, new object[1] { "escape" });
			if (keyControlEscape == null)
			{
				PFLog.UnityModManager.Error("Value keyControlEscape is null.");
			}
			hasErrors = hasErrors || keyControlCtrl == null || keyControlShift == null || keyControlAlt == null;
		}
	}

	public static bool Ctrl()
	{
		if (LegacyInputDisabled)
		{
			return !hasErrors && (bool)isPressedPI.GetValue(keyControlCtrl, null);
		}
		return Input.GetKey((KeyCode)306) || Input.GetKey((KeyCode)305);
	}

	public static bool Shift()
	{
		if (LegacyInputDisabled)
		{
			return !hasErrors && (bool)isPressedPI.GetValue(keyControlShift, null);
		}
		return Input.GetKey((KeyCode)304) || Input.GetKey((KeyCode)303);
	}

	public static bool Alt()
	{
		if (LegacyInputDisabled)
		{
			return !hasErrors && (bool)isPressedPI.GetValue(keyControlAlt, null);
		}
		return Input.GetKey((KeyCode)308) || Input.GetKey((KeyCode)307);
	}

	public void Change(KeyCode key, bool ctrl, bool shift, bool alt)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		Change(key, (byte)((ctrl ? 1 : 0) + (shift ? 2 : 0) + (alt ? 4 : 0)));
	}

	public void Change(KeyCode key, byte modifier = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		keyCode = key;
		modifiers = modifier;
		m_Index = -1;
		m_KeyControl = null;
	}

	public bool Pressed()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (int)keyCode != 0 && ((modifiers & 1) == 0 || Ctrl()) && ((modifiers & 2) == 0 || Shift()) && ((modifiers & 4) == 0 || Alt());
		if (LegacyInputDisabled)
		{
			return !hasErrors && flag && KeyControl != KeyControlZero && (bool)isPressedPI.GetValue(KeyControl, null);
		}
		return flag && Input.GetKey(keyCode);
	}

	public bool Down()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (int)keyCode != 0 && ((modifiers & 1) == 0 || Ctrl()) && ((modifiers & 2) == 0 || Shift()) && ((modifiers & 4) == 0 || Alt());
		if (LegacyInputDisabled)
		{
			return !hasErrors && flag && KeyControl != KeyControlZero && (bool)wasPressedThisFramePI.GetValue(KeyControl, null);
		}
		return flag && Input.GetKeyDown(keyCode);
	}

	public bool Up()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		bool flag = (int)keyCode != 0 && ((modifiers & 1) == 0 || Ctrl()) && ((modifiers & 2) == 0 || Shift()) && ((modifiers & 4) == 0 || Alt());
		if (LegacyInputDisabled)
		{
			return !hasErrors && flag && KeyControl != KeyControlZero && (bool)wasReleasedThisFramePI.GetValue(KeyControl, null);
		}
		return flag && Input.GetKeyUp(keyCode);
	}
}
