using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ChoseControllerMode;
using Kingmaker.UI.Common;
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

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		m_HintLabel.text = UIStrings.Instance.ControllerModeTexts.PressAnyKeyText;
		base.gameObject.SetActive(value: true);
		AddDisposable(GamePad.Instance.PushLayer(GetInputLayer()));
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(OnLateUpdate));
		AddDisposable(base.ViewModel.GamepadDisconnected.Subscribe(base.ViewModel.SetKeyboardMode));
	}

	private InputLayer GetInputLayer()
	{
		InputLayer inputLayer = new InputLayer();
		inputLayer.ContextName = "ChoseControllerModeWindowView";
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 8);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 9);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 10);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 11);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 16);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 17);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 12);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 14);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 13);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 15);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 7);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 4);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 5);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 6);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 18);
		inputLayer.AddButton(delegate(InputActionEventData eventData)
		{
			SetGamepadMode(eventData);
		}, 19);
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
			base.ViewModel.SetGamepadMode();
		}
	}

	private void OnLateUpdate()
	{
		if (UIUtility.IsAnyKeyboardKeyDown())
		{
			base.ViewModel.SetKeyboardMode();
		}
	}

	protected override void DestroyViewImplementation()
	{
		GamePad.Instance.PopLayer(m_InputLayer);
		base.gameObject.SetActive(value: false);
		UILog.ViewUnbinded("ChoseControllerModeWindowView");
	}
}
