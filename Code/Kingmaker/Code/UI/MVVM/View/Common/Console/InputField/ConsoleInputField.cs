using System;
using Kingmaker.Localization.Enums;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;

public class ConsoleInputField : MonoBehaviour, IConsoleNavigationEntity, IConsoleEntity, IConfirmClickHandler
{
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TMP_InputField m_InputField;

	[SerializeField]
	private GameObject m_EditGameObject;

	private TextMeshProUGUI m_PlaceholderTextLabel;

	private string m_TittleText;

	private string m_InputText;

	private string m_PlaceholderText;

	private uint m_MaxTextLength = 128u;

	private Locale m_KeyboardLanguage;

	private Action<string> m_OnEndEdit;

	private IVirtualKeyboard m_VirtualKeyboard;

	private BoolReactiveProperty m_IsEditing = new BoolReactiveProperty();

	private TextMeshProUGUI PlaceholderTextLabel => m_PlaceholderTextLabel.Or(null) ?? (m_PlaceholderTextLabel = m_InputField.placeholder.GetComponent<TextMeshProUGUI>());

	private IVirtualKeyboard VirtualKeyboard
	{
		get
		{
			IVirtualKeyboard virtualKeyboard = m_VirtualKeyboard;
			if (virtualKeyboard == null)
			{
				IVirtualKeyboard virtualKeyboard3;
				IVirtualKeyboard virtualKeyboard2;
				if (!ApplicationHelper.IsRunOnSteamDeck)
				{
					virtualKeyboard2 = new PCVirtualKeyboard(m_InputField);
					virtualKeyboard3 = virtualKeyboard2;
				}
				else
				{
					virtualKeyboard2 = SteamDeckVirtualKeyboard.Create();
					virtualKeyboard3 = virtualKeyboard2;
				}
				virtualKeyboard2 = virtualKeyboard3;
				m_VirtualKeyboard = virtualKeyboard3;
				virtualKeyboard = virtualKeyboard2;
			}
			return virtualKeyboard;
		}
	}

	public BoolReactiveProperty IsEditing => m_IsEditing;

	public TMP_InputField InputField => m_InputField;

	public string Text
	{
		get
		{
			return m_InputField.text;
		}
		set
		{
			m_InputText = value;
			m_InputField.text = value ?? "";
		}
	}

	public void Bind(string defaultText, Action<string> onEndEditAction)
	{
		m_OnEndEdit = onEndEditAction;
		Text = defaultText;
	}

	public void SetTittle(string text)
	{
		m_TittleText = text;
	}

	public void SetPlaceholderText(string text)
	{
		m_PlaceholderText = text;
		PlaceholderTextLabel.text = text ?? "";
	}

	public void SetLanguage(Locale language)
	{
		m_KeyboardLanguage = language;
	}

	public void SetMaxTextLength(uint maxTextLength)
	{
		m_MaxTextLength = maxTextLength;
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocused(value);
		if (value)
		{
			Select();
		}
		else
		{
			Abort();
		}
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public void Select()
	{
		if (m_IsEditing.Value)
		{
			Abort();
			return;
		}
		m_EditGameObject.Or(null)?.SetActive(value: true);
		m_IsEditing.Value = true;
		VirtualKeyboard.OpenKeyboard(OnKeyboardEditSucceeded, OnKeyboardEditDeclined, m_TittleText, m_InputText, m_PlaceholderText, m_KeyboardLanguage, m_MaxTextLength);
	}

	public void ActivateInputField()
	{
		m_InputField.ActivateInputField();
	}

	public void MoveTextStart(bool value)
	{
		m_InputField.MoveTextStart(value);
	}

	private void OnKeyboardEditSucceeded(string text)
	{
		Abort();
		Text = text;
		m_OnEndEdit?.Invoke(text);
	}

	private void OnKeyboardEditDeclined()
	{
		Abort();
		m_OnEndEdit?.Invoke(m_InputText);
	}

	public void Abort()
	{
		m_EditGameObject.Or(null)?.SetActive(value: false);
		if (m_InputField.isFocused)
		{
			m_InputField.DeactivateInputField();
			VirtualKeyboard.Abort();
		}
		if (m_IsEditing.Value)
		{
			m_IsEditing.Value = false;
		}
	}

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		Select();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
