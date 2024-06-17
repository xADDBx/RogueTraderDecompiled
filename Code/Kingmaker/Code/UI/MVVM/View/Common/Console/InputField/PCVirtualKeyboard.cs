using System;
using Kingmaker.Localization.Enums;
using Owlcat.Runtime.Core.Utility;
using TMPro;

namespace Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;

public class PCVirtualKeyboard : IVirtualKeyboard
{
	private TMP_InputField m_InputField;

	private TextMeshProUGUI m_PlaceholderTextLabel;

	public PCVirtualKeyboard(TMP_InputField inputField)
	{
		m_InputField = inputField;
		m_PlaceholderTextLabel = m_InputField.placeholder.Or(null)?.GetComponent<TextMeshProUGUI>();
	}

	public void OpenKeyboard(Action<string> successCallback, Action cancelCallback, string titleText, string inputText, string placeholderText, Locale language, uint maxTextLength)
	{
		m_InputField.ActivateInputField();
		m_InputField.onEndEdit.AddListener(delegate(string text)
		{
			successCallback?.Invoke(text);
		});
		m_InputField.characterLimit = (int)maxTextLength;
		m_InputField.text = inputText ?? "";
		if (m_PlaceholderTextLabel != null)
		{
			m_PlaceholderTextLabel.text = placeholderText ?? "";
		}
	}

	public void Abort()
	{
		m_InputField.DeactivateInputField();
		m_InputField.onEndEdit.RemoveAllListeners();
	}
}
