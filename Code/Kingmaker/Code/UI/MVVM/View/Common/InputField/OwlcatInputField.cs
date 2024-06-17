using System;
using Kingmaker.Code.UI.MVVM.View.Common.Console.InputField;
using Kingmaker.Localization.Enums;
using Kingmaker.UI.InputSystems;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.ConsoleTools;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.Common.InputField;

public class OwlcatInputField : MonoBehaviour, IConfirmClickHandler, IConsoleEntity, IScrollHandler, IEventSystemHandler, IConsolePointerLeftClickEvent, IConsoleNavigationEntity
{
	[SerializeField]
	private TMP_InputField m_InputField;

	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	private TextMeshProUGUI m_PlaceholderTextLabel;

	private string m_TittleText;

	private string m_InputText;

	private string m_PlaceholderText;

	private uint m_MaxTextLength = 128u;

	private bool m_IsEnteredWithMouse;

	private CompositeDisposable m_Disposables = new CompositeDisposable();

	private IDisposable m_Disposable;

	private InputLayer m_InputLayer;

	public static readonly string InputLayerContextName = "InputField";

	private Locale m_KeyboardLanguage;

	private Action<string> m_OnEndEdit;

	private bool m_IsEditing;

	private IVirtualKeyboard m_VirtualKeyboard;

	public ReactiveCommand PointerLeftClickCommand { get; } = new ReactiveCommand();


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

	private void OnEnable()
	{
		m_Disposable = m_Button.OnLeftClickAsObservable().Subscribe(delegate
		{
			if (!m_IsEditing)
			{
				m_IsEnteredWithMouse = true;
				PointerLeftClickCommand.Execute();
				SelectInputFiled();
			}
		});
	}

	private void OnDisable()
	{
		m_Disposable?.Dispose();
		Abort();
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
		if (!m_IsEnteredWithMouse || !value)
		{
			m_Button.SetFocused(value);
		}
		if (!value)
		{
			Abort();
		}
	}

	public bool IsValid()
	{
		return base.isActiveAndEnabled;
	}

	public void SelectInputFiled()
	{
		if (m_IsEditing)
		{
			Abort();
			return;
		}
		m_IsEditing = true;
		m_InputLayer = new InputLayer
		{
			ContextName = InputLayerContextName
		};
		m_Disposables.Add(EscHotkeyManager.Instance.Subscribe(Abort));
		m_Disposables.Add(m_InputLayer.AddButton(delegate
		{
			Abort();
		}, 9));
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			SelectInputFiled();
		}, 10, InputActionEventType.ButtonJustReleased);
		if (m_ConfirmHint != null && Game.Instance.ControllerMode == Game.ControllerModeType.Gamepad)
		{
			m_Disposables.Add(m_ConfirmHint.Bind(inputBindStruct));
		}
		m_Disposables.Add(inputBindStruct);
		m_Disposables.Add(m_InputLayer.AddAxis(OnConsoleScroll, 3));
		m_Disposables.Add(GamePad.Instance.PushLayer(m_InputLayer));
		m_Button.SetActiveLayer("On");
		m_InputField.OnSelect(null);
		VirtualKeyboard.OpenKeyboard(OnKeyboardEditSucceeded, OnKeyboardEditDeclined, m_TittleText, m_InputText, m_PlaceholderText, m_KeyboardLanguage, m_MaxTextLength);
	}

	public void OnScroll(PointerEventData eventData)
	{
		m_InputField.OnScroll(eventData);
	}

	private void OnConsoleScroll(InputActionEventData inputActionEventData, float value)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.scrollDelta = new Vector2(0f, value * m_InputField.scrollSensitivity);
		m_InputField.OnScroll(pointerEventData);
	}

	public void Abort()
	{
		if (m_IsEditing)
		{
			m_IsEditing = false;
			m_Disposables.Clear();
			m_Button.SetActiveLayer("Off");
			m_InputField.OnDeselect(null);
			m_InputField.ReleaseSelection();
			if (!EventSystem.current.alreadySelecting)
			{
				EventSystem.current.SetSelectedGameObject(null);
			}
			VirtualKeyboard.Abort();
			m_IsEnteredWithMouse = false;
		}
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

	public bool CanConfirmClick()
	{
		return true;
	}

	public void OnConfirmClick()
	{
		if (!TrySubmitInputField())
		{
			SelectInputFiled();
		}
	}

	private bool TrySubmitInputField()
	{
		if (m_IsEditing)
		{
			bool flag = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			bool keyDown = Input.GetKeyDown(KeyCode.Return);
			if ((m_IsEnteredWithMouse || !m_InputField.multiLine || flag) && keyDown)
			{
				m_InputField.OnSubmit(null);
				return true;
			}
		}
		return false;
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
