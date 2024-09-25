using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.Titles;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Titles;

public class TitlesConsoleView : TitlesBaseView
{
	[SerializeField]
	private ConsoleHint m_SpeedUPHint;

	[SerializeField]
	private ConsoleHint m_CloseHint;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(m_NavigationBehaviour = new GridConsoleNavigationBehaviour());
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "TitlesView"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			base.ViewModel.OpenCancelSettingsDialog();
		}, 9, InputActionEventType.ButtonJustLongPressed);
		AddDisposable(inputBindStruct);
		m_CloseHint.Bind(inputBindStruct);
		m_CloseHint.SetLabel(UIStrings.Instance.CommonTexts.SkipHold);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			SpeedUp(state: true);
		}, 8);
		AddDisposable(inputBindStruct2);
		m_SpeedUPHint.Bind(inputBindStruct2);
		m_SpeedUPHint.SetLabel(UIStrings.Instance.CommonTexts.HoldGamepadButton.Text + " " + UIStrings.Instance.Credits.SpeedUp.Text);
		AddDisposable(m_InputLayer.AddButton(delegate
		{
			SpeedUp(state: false);
		}, 8, InputActionEventType.ButtonJustReleased));
		AddDisposable(m_InputLayer.AddButton(delegate
		{
			SpeedUp(state: false);
		}, 8, InputActionEventType.ButtonLongPressJustReleased));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
