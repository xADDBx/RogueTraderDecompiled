using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ChoseControllerMode;

public class GamepadConnectedInKeyboardModeWindowView : ViewBase<GamepadConnectDisconnectVM>
{
	[SerializeField]
	private ConsoleHint m_ConfirmHint;

	private BoolReactiveProperty m_CanConfirm = new BoolReactiveProperty();

	[SerializeField]
	private ConsoleHint m_DeclineHint;

	private BoolReactiveProperty m_CanDecline = new BoolReactiveProperty();

	[SerializeField]
	private WindowAnimator m_WindowAnimator;

	[SerializeField]
	public TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	public TextMeshProUGUI m_HintLabel;

	private bool m_IsOpened;

	private float m_MinimumDelayBetweenPrompts = 1f;

	private float m_LastInput = float.MinValue;

	private InputLayer m_InputLayer;

	private InputLayer m_WindowInputLayer;

	private CompositeDisposable m_Disposable;

	public void Initialize()
	{
		m_WindowAnimator.Initialize();
	}

	protected override void BindViewImplementation()
	{
		if (!base.ViewModel.IsControllerOverride)
		{
			m_HeaderLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadConnectedHeaderText;
			m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.GamepadConnectedText;
			m_InputLayer = GetInputLayer();
			AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.GamepadConnected, delegate
			{
				GamepadConnected(null);
			}));
			AddDisposable(ObservableExtensions.Subscribe(base.ViewModel.GamepadDisconnected, delegate
			{
				GamepadDisconnected();
			}));
			GamepadConnectedOnStart();
		}
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = new InputLayer();
		inputLayer.ContextName = "ChooseControllerModeWindowView";
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 8);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 9);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 10);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 11);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 16);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 17);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 12);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 14);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 13);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 15);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 7);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 4);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 5);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 6);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 18);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			GamepadConnected(eventData);
		}, 19);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			GamepadConnected(eventData);
		}, 0);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			GamepadConnected(eventData);
		}, 1);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			GamepadConnected(eventData);
		}, 2);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			GamepadConnected(eventData);
		}, 3);
		return inputLayer;
	}

	private void CreateNavigation()
	{
		m_WindowInputLayer = new InputLayer
		{
			ContextName = "GamepadConnectedInKeyboardModeWindowView"
		};
		if (m_Disposable == null)
		{
			m_Disposable = new CompositeDisposable();
		}
		else
		{
			m_Disposable.Clear();
		}
		m_Disposable.Add(m_ConfirmHint.Bind(m_WindowInputLayer.AddButton(delegate
		{
			SwitchControlMode();
		}, 8, m_CanConfirm)));
		m_ConfirmHint.SetLabel(UIStrings.Instance.ControllerModeTexts.ConfirmSwitchText);
		m_Disposable.Add(m_DeclineHint.Bind(m_WindowInputLayer.AddButton(delegate
		{
			Decline();
		}, 9, m_CanDecline)));
		m_DeclineHint.SetLabel(UIStrings.Instance.CommonTexts.Cancel);
		GamePad.Instance.PushLayer(m_WindowInputLayer);
	}

	private void Decline()
	{
		base.ViewModel.DeclineController();
		Hide();
	}

	private void GamepadConnected(InputActionEventData? eventData)
	{
		if (eventData.HasValue && eventData.Value.GetCurrentInputSources().Any((InputActionSourceData c) => c.controllerType == ControllerType.Keyboard))
		{
			return;
		}
		if (!RootUIContext.CanChangeInput())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(UIStrings.Instance.ControllerModeTexts.CantChangeInput);
			});
			return;
		}
		GamePad.Instance.PushLayer(m_InputLayer);
		if (!m_IsOpened && Time.realtimeSinceStartup > m_LastInput + m_MinimumDelayBetweenPrompts)
		{
			Show();
		}
		m_LastInput = Time.realtimeSinceStartup;
	}

	private void GamepadConnectedOnStart()
	{
		if (ReInput.controllers.Joysticks.Count != 0)
		{
			GamePad.Instance.PushLayer(m_InputLayer);
		}
	}

	private void SwitchControlMode()
	{
		m_HeaderLabel.text = UIStrings.Instance.ControllerModeTexts.ChangeInputProcess;
		m_HintLabel.text = string.Empty;
		HandleCurrentState(value: false);
		DelayedSwitch();
	}

	private void DelayedSwitch()
	{
		DelayedInvoker.InvokeInFrames(base.ViewModel.SwitchControlMode, 1);
	}

	private void GamepadDisconnected()
	{
		GamePad.Instance.PopLayer(m_InputLayer);
		if (m_IsOpened)
		{
			Hide();
		}
	}

	private void Show()
	{
		base.gameObject.SetActive(value: true);
		m_WindowAnimator.AppearAnimation();
		m_IsOpened = true;
		CreateNavigation();
		HandleCurrentState(value: true);
	}

	private void Hide()
	{
		m_WindowAnimator.DisappearAnimation(delegate
		{
			base.gameObject.SetActive(value: false);
			m_IsOpened = false;
			GamePad.Instance.PopLayer(m_WindowInputLayer);
			m_Disposable?.Clear();
			HandleCurrentState(value: false);
		});
	}

	private void HandleCurrentState(bool value)
	{
		Game.Instance.Keyboard.Disabled.SetValue(value);
		Game.Instance.RequestPauseUi(value);
		m_CanConfirm.Value = value;
		m_CanDecline.Value = value;
	}

	protected override void DestroyViewImplementation()
	{
		m_Disposable?.Dispose();
		m_Disposable = null;
	}
}
