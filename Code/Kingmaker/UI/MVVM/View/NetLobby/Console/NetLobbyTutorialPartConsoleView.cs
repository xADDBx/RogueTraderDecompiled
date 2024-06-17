using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.MVVM.View.NetLobby.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.NetLobby.Console;

public class NetLobbyTutorialPartConsoleView : NetLobbyTutorialPartBaseView
{
	[SerializeField]
	private ConsoleHintsWidget m_CommonHintsWidget;

	private InputLayer m_InputLayer;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		CreateInput();
	}

	private void CreateInput()
	{
		m_InputLayer = new InputLayer
		{
			ContextName = "NetLobbyTutorial"
		};
		AddDisposable(m_CommonHintsWidget.BindHint(m_InputLayer.AddButton(delegate
		{
			ShowBlock();
		}, 8), UIStrings.Instance.CommonTexts.Accept));
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
