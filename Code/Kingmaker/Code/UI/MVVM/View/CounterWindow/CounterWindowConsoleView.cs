using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Owlcat.Runtime.UniRx;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.CounterWindow;

public class CounterWindowConsoleView : CounterWindowPCView
{
	[Header("ConsoleInput")]
	[SerializeField]
	private ConsoleHintsWidget m_ConsoleHintsWidget;

	[SerializeField]
	private ConsoleHint m_AcceptHint;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "CounterWindow"
		});
		AddDisposable(m_InputLayer.AddAxis(delegate(InputActionEventData _, float value)
		{
			OnLeftStickX(value);
		}, 0, repeat: true));
		AddDisposable(m_InputLayer.AddButton(delegate(InputActionEventData handler)
		{
			ChangeValue(Mathf.Max(1, Mathf.FloorToInt((float)handler.GetButtonTimePressed())));
		}, 5, InputActionEventType.ButtonRepeating));
		AddDisposable(m_InputLayer.AddButton(delegate(InputActionEventData handler)
		{
			ChangeValue(-1 * Mathf.Max(1, Mathf.FloorToInt((float)handler.GetButtonTimePressed())));
		}, 4, InputActionEventType.ButtonRepeating));
		int mediumShift = GetMediumShiftAmount();
		if (mediumShift > 1)
		{
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				ChangeValue(mediumShift);
			}, 15, InputActionEventType.ButtonRepeating), $"+{mediumShift}"));
			AddDisposable(m_ConsoleHintsWidget.BindHint(m_InputLayer.AddButton(delegate
			{
				ChangeValue(-mediumShift);
			}, 14, InputActionEventType.ButtonRepeating), $"-{mediumShift}"));
		}
		AddDisposable(m_CloseHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Close();
		}, 9)));
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.CloseWindow);
		AddDisposable(m_AcceptHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Accept();
		}, 8)));
		m_AcceptHint.SetLabel(GetOperationButtonText());
		DelayedInvoker.InvokeInFrames(delegate
		{
			m_CloseHint.transform.SetAsLastSibling();
			m_AcceptHint.transform.SetAsFirstSibling();
		}, 1);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	private void OnLeftStickX(float x)
	{
		float num = ((Mathf.Abs(x) > 0.1f) ? Mathf.Sign(x) : 0f);
		ChangeValue((int)num);
	}

	private void ChangeValue(int value)
	{
		m_CountSlider.value += value;
	}

	private int GetMediumShiftAmount()
	{
		return Mathf.Min((base.ViewModel.MaxValue - 1) / 2, 10);
	}
}
