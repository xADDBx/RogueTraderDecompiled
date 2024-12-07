using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.Controls.Button;
using Owlcat.Runtime.UI.Controls.Other;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View.ChoseControllerMode;

public class GamepadDisconnectedInGamepadModeWindowView : ViewBase<GamepadConnectDisconnectVM>
{
	[SerializeField]
	private OwlcatButton m_ConfirmButton;

	[SerializeField]
	private OwlcatButton m_DeclineButton;

	[SerializeField]
	private TextMeshProUGUI m_ConfirmLabel;

	[SerializeField]
	private TextMeshProUGUI m_DeclineLabel;

	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	private TextMeshProUGUI m_BodyLabel;

	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	private InputLayer m_InputLayer;

	private readonly BoolReactiveProperty m_IsGamepadConnected = new BoolReactiveProperty(initialValue: true);

	private bool m_IsOpened;

	private EventSystem m_StoredGamepadModeEventSystem;

	private IDisposable m_KeyboardInput;

	private CompositeDisposable m_EscSubscription = new CompositeDisposable();

	private bool m_CursorVisible;

	public void Initialize()
	{
		m_WindowAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		if (!base.ViewModel.IsControllerOverride)
		{
			m_BodyLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadDisconnectedHeaderText;
			m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadDisconnectedText;
			AddDisposable(base.ViewModel.GamepadConnected.Subscribe(GamepadConnected));
			AddDisposable(base.ViewModel.GamepadDisconnected.Subscribe(GamepadDisconnected));
			AddDisposable(m_KeyboardInput = MainThreadDispatcher.LateUpdateAsObservable().Subscribe(OnLateUpdate));
			m_InputLayer = new InputLayer
			{
				ContextName = "Gamepad Disconnected Layer"
			};
			AddDisposable(m_ConfirmButton.OnLeftClickAsObservable().Subscribe(SwitchControlMode));
			AddDisposable(m_DeclineButton.OnLeftClickAsObservable().Subscribe(GamepadConnected));
			AddDisposable(m_IsGamepadConnected.Subscribe(m_DeclineButton.SetInteractable));
			m_ConfirmLabel.text = UIStrings.Instance.ControllerModeTexts.ConfirmSwitchText.Text;
			m_DeclineLabel.text = UIStrings.Instance.CommonTexts.Cancel.Text;
		}
	}

	protected override void DestroyViewImplementation()
	{
		if (m_IsOpened)
		{
			Hide();
		}
		ClearKeyboard();
	}

	private void GamepadConnected()
	{
		if (m_IsOpened)
		{
			Hide();
			DelayedKeyboardSub();
		}
	}

	private void GamepadDisconnected()
	{
		if (!RootUIContext.CanChangeInput())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ControllerModeTexts.CantChangeInput);
			});
		}
		else
		{
			Show();
		}
	}

	private void Show()
	{
		ClearKeyboard();
		m_StoredGamepadModeEventSystem = EventSystem.current;
		m_StoredGamepadModeEventSystem.enabled = false;
		base.gameObject.SetActive(value: true);
		UISounds.Instance.Sounds.MessageBox.MessageBoxShow.Play();
		m_WindowAnimator.AppearAnimation();
		m_IsOpened = true;
		SetupHint();
		HandleCurrentState(value: true);
		m_CursorVisible = Cursor.visible;
		Cursor.visible = true;
	}

	private void SetupHint()
	{
		m_IsGamepadConnected.Value = ReInput.controllers.joystickCount > 0;
		m_EscSubscription.Add(m_InputLayer.AddButton(delegate
		{
			GamepadConnected();
		}, 9, m_IsGamepadConnected, InputActionEventType.ButtonJustReleased));
		GamePad.Instance.PushLayer(m_InputLayer);
	}

	private void OnLateUpdate()
	{
		if (!IgnoreKeyPressed() && Input.GetKeyDown(KeyCode.Space))
		{
			GamepadDisconnected();
		}
	}

	private bool IgnoreKeyPressed()
	{
		if (!RootUIContext.Instance.IsChargenShown && !RootUIContext.Instance.IsBugReportOpen && !RootUIContext.Instance.SaveLoadIsShown && RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.Inventory)
		{
			return RootUIContext.Instance.CreditsAreShown;
		}
		return true;
	}

	private void Hide()
	{
		m_WindowAnimator.DisappearAnimation(delegate
		{
			UISounds.Instance.Sounds.MessageBox.MessageBoxHide.Play();
			base.gameObject.SetActive(value: false);
			m_StoredGamepadModeEventSystem.enabled = true;
			m_StoredGamepadModeEventSystem = null;
		});
		m_IsOpened = false;
		GamePad.Instance.PopLayer(m_InputLayer);
		HandleCurrentState(value: false);
		m_EscSubscription.Clear();
		Cursor.visible = m_CursorVisible;
	}

	private void ClearKeyboard()
	{
		RemoveDisposable(m_KeyboardInput);
		m_KeyboardInput?.Dispose();
	}

	private void HandleCurrentState(bool value)
	{
		Game.Instance.Keyboard.Disabled.SetValue(value);
		Game.Instance.RequestPauseUi(value);
		m_ConfirmButton.Interactable = value;
		m_DeclineButton.Interactable = value;
	}

	private void SwitchControlMode()
	{
		m_BodyLabel.text = UIStrings.Instance.ControllerModeTexts.ChangeInputProcess;
		m_HintLabel.text = string.Empty;
		HandleCurrentState(value: false);
		DelayedSwitch();
	}

	private void DelayedSwitch()
	{
		DelayedInvoker.InvokeInFrames(base.ViewModel.SwitchControlMode, 1);
	}

	private void DelayedKeyboardSub()
	{
		DelayedInvoker.InvokeInFrames(Sub, 1);
	}

	private void Sub()
	{
		AddDisposable(m_KeyboardInput = MainThreadDispatcher.LateUpdateAsObservable().Subscribe(OnLateUpdate));
	}
}
