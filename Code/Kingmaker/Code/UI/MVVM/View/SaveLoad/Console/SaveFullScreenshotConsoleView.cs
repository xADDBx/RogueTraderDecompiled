using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.SaveLoad.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.SaveLoad.Console;

public class SaveFullScreenshotConsoleView : SaveFullScreenshotBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	private InputLayer m_InputLayer;

	public const string InputLayerContextName = "SaveFullScreenshotConsoleView";

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "SaveFullScreenshotConsoleView"
		};
		AddDisposable(m_HintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.HideScreenshot();
		}, 9, InputActionEventType.ButtonJustReleased), UIStrings.Instance.CommonTexts.CloseWindow, ConsoleHintsWidget.HintPosition.Right));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
