using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Vendor.Console;

public class VendorTransitionWindowConsoleView : VendorTransitionWindowView
{
	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateNavigation();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Transition Window Console View"
		});
		CreateInput();
	}

	private void CreateInput()
	{
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Deal();
		}, 8, InputActionEventType.ButtonJustLongPressed), UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ChangeSliderValue(1);
		}, 5), UIStrings.Instance.CommonTexts.Increase));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ChangeSliderValue(-1);
		}, 4), UIStrings.Instance.CommonTexts.Decrease));
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			Close();
		}, 9, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		Close();
	}

	private void ChangeSliderValue(int value)
	{
		m_Slider.value += value;
	}
}
