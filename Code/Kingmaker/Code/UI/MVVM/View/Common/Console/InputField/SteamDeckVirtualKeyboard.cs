using System;
using Kingmaker.Localization.Enums;
using Steamworks;

namespace Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;

public class SteamDeckVirtualKeyboard : IVirtualKeyboard
{
	private static SteamDeckVirtualKeyboard s_VirtualKeyboard;

	private readonly Callback<GamepadTextInputDismissed_t> m_GamepadTextInputDismissed;

	private Action<string> m_SuccessCallback;

	private Action m_CancelCallback;

	private uint m_MaxTextLength;

	private bool m_IsWaitingForCallback;

	public static SteamDeckVirtualKeyboard Create()
	{
		if (s_VirtualKeyboard == null)
		{
			s_VirtualKeyboard = new SteamDeckVirtualKeyboard();
		}
		return s_VirtualKeyboard;
	}

	private SteamDeckVirtualKeyboard()
	{
		m_GamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnGamepadTextInputDismissed);
	}

	public void OpenKeyboard(Action<string> successCallback, Action cancelCallback, string titleText, string inputText, string placeholderText, Locale language, uint maxTextLength)
	{
		if (m_IsWaitingForCallback)
		{
			PFLog.UI.Warning("Opening the virtual keyboard while waiting for previous input processing");
		}
		if (SteamUtils.ShowGamepadTextInput(EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, placeholderText ?? " ", maxTextLength, inputText))
		{
			m_SuccessCallback = successCallback;
			m_CancelCallback = cancelCallback;
			m_MaxTextLength = maxTextLength;
			m_IsWaitingForCallback = true;
		}
	}

	public void Abort()
	{
	}

	private void OnGamepadTextInputDismissed(GamepadTextInputDismissed_t result)
	{
		if (!m_IsWaitingForCallback)
		{
			HandleUnexpectedCallback(result);
			return;
		}
		if (result.m_bSubmitted)
		{
			SteamUtils.GetEnteredGamepadTextInput(out var pchText, m_MaxTextLength);
			m_SuccessCallback?.Invoke(pchText);
		}
		else
		{
			m_CancelCallback?.Invoke();
		}
		m_IsWaitingForCallback = false;
	}

	private void HandleUnexpectedCallback(GamepadTextInputDismissed_t result)
	{
		if (result.m_bSubmitted)
		{
			SteamUtils.GetEnteredGamepadTextInput(out var pchText, m_MaxTextLength);
			PFLog.UI.Warning("Received unexpected callback, submitted: true, text: " + pchText);
		}
		else
		{
			PFLog.UI.Warning("Received unexpected callback, submitted: false");
		}
	}
}
