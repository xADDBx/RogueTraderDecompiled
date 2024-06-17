using System;
using Kingmaker.Localization.Enums;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UniRx;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;

public class CrossPlatformConsoleVirtualKeyboard : IVirtualKeyboard
{
	private IVirtualKeyboard m_Keyboard;

	private InputLayer m_InputLayer;

	public static readonly string InputLayerContextName = "Virtual Keyboard";

	public CrossPlatformConsoleVirtualKeyboard(TMP_InputField inputField)
	{
		CreateInputLayer();
	}

	public void OpenKeyboard(Action<string> successCallback, Action cancelCallback, string titleText, string inputText, string placeholderText, Locale language, uint maxTextLength)
	{
		if (m_Keyboard == null)
		{
			successCallback?.Invoke(inputText + " (keyboard not supported)");
			return;
		}
		GamePad.Instance.PushLayer(m_InputLayer);
		if (maxTextLength == 0)
		{
			maxTextLength = 128u;
		}
		successCallback = (Action<string>)Delegate.Combine(successCallback, (Action<string>)delegate
		{
			Abort();
		});
		cancelCallback = (Action)Delegate.Combine(cancelCallback, (Action)delegate
		{
			Abort();
		});
		m_Keyboard.OpenKeyboard(successCallback, cancelCallback, titleText, inputText, placeholderText, language, maxTextLength);
	}

	private void CreateInputLayer()
	{
		if (m_InputLayer == null)
		{
			m_InputLayer = new InputLayer
			{
				ContextName = InputLayerContextName
			};
			m_InputLayer.AddButton(delegate
			{
				Abort();
			}, 9);
		}
	}

	public void Abort()
	{
		m_Keyboard?.Abort();
		DelayedInvoker.InvokeInTime(delegate
		{
			GamePad.Instance.PopLayer(m_InputLayer);
		}, 0.5f);
	}
}
