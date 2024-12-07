using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Common;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using Rewired;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ChoseControllerMode;

public class ChoseControllerModeWindowView : ViewBase<GamepadConnectDisconnectVM>
{
	[SerializeField]
	private TextMeshProUGUI m_HintLabel;

	[SerializeField]
	[AkEventReference]
	private string m_ShowSound;

	[SerializeField]
	[AkEventReference]
	private string m_HideSound;

	[SerializeField]
	[AkEventReference]
	private string m_ClickSound;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.PressAnyKeyText;
		base.gameObject.SetActive(value: true);
		if (!string.IsNullOrWhiteSpace(m_ShowSound))
		{
			SoundEventsManager.PostEvent(m_ShowSound, base.gameObject);
		}
		AddDisposable(GamePad.Instance.PushLayer(GetInputLayer()));
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(OnLateUpdate));
		AddDisposable(base.ViewModel.GamepadDisconnected.Subscribe(base.ViewModel.SetKeyboardMode));
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.PopLayer(m_InputLayer);
		if (!string.IsNullOrWhiteSpace(m_HideSound))
		{
			SoundEventsManager.PostEvent(m_HideSound, base.gameObject);
		}
		base.gameObject.SetActive(value: false);
		UILog.ViewUnbinded("ChoseControllerModeWindowView");
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = new InputLayer();
		inputLayer.ContextName = "ChoseControllerModeWindowView";
		inputLayer.AddButton(SetGamepadMode, 8);
		inputLayer.AddButton(SetGamepadMode, 9);
		inputLayer.AddButton(SetGamepadMode, 10);
		inputLayer.AddButton(SetGamepadMode, 11);
		inputLayer.AddButton(SetGamepadMode, 16);
		inputLayer.AddButton(SetGamepadMode, 17);
		inputLayer.AddButton(SetGamepadMode, 12);
		inputLayer.AddButton(SetGamepadMode, 14);
		inputLayer.AddButton(SetGamepadMode, 13);
		inputLayer.AddButton(SetGamepadMode, 15);
		inputLayer.AddButton(SetGamepadMode, 7);
		inputLayer.AddButton(SetGamepadMode, 4);
		inputLayer.AddButton(SetGamepadMode, 5);
		inputLayer.AddButton(SetGamepadMode, 6);
		inputLayer.AddButton(SetGamepadMode, 18);
		inputLayer.AddButton(SetGamepadMode, 19);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 0);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 1);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 2);
		inputLayer.AddAxis(delegate(InputActionEventData eventData, float __)
		{
			SetGamepadMode(eventData);
		}, 3);
		return inputLayer;
	}

	private void SetGamepadMode(InputActionEventData eventData)
	{
		if (eventData.IsCurrentInputSource(ControllerType.Joystick))
		{
			if (!string.IsNullOrWhiteSpace(m_ClickSound))
			{
				SoundEventsManager.PostEvent(m_ClickSound, base.gameObject);
			}
			base.ViewModel.SetGamepadMode();
		}
	}

	private void OnLateUpdate()
	{
		if (UIUtility.IsAnyKeyboardKeyDown())
		{
			if (!string.IsNullOrWhiteSpace(m_ClickSound))
			{
				SoundEventsManager.PostEvent(m_ClickSound, base.gameObject);
			}
			base.ViewModel.SetKeyboardMode();
		}
	}
}
