using System;
using UnityEngine;

namespace Kingmaker.IngameConsole;

public class IngameConsoleKeybinds : MonoBehaviour
{
	private IDisposable m_BindDisposable;

	private void Start()
	{
		m_BindDisposable = Game.Instance.Keyboard.Bind("SwitchUberConsole", SwitchUberConsole);
	}

	private void OnDestroy()
	{
		m_BindDisposable?.Dispose();
		m_BindDisposable = null;
	}

	public static void Refresh()
	{
		IngameConsoleKeybinds ingameConsoleKeybinds = UnityEngine.Object.FindObjectOfType<IngameConsoleKeybinds>();
		if ((bool)ingameConsoleKeybinds)
		{
			ingameConsoleKeybinds.m_BindDisposable = Game.Instance.Keyboard.Bind("SwitchUberConsole", SwitchUberConsole);
		}
	}

	private static void SwitchUberConsole()
	{
		UberLoggerAppWindow.Instance.IsShown = !UberLoggerAppWindow.Instance.IsShown;
	}
}
