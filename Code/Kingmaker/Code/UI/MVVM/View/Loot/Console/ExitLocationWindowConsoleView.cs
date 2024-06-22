using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Loot.Console;

public class ExitLocationWindowConsoleView : ExitLocationWindowBaseView
{
	[SerializeField]
	protected ConsoleHint m_AcceptHint;

	[SerializeField]
	protected ConsoleHint m_DeclineHint;

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
			ContextName = "Exit Location Window"
		};
		AddDisposable(m_DeclineHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Decline();
		}, 9)));
		m_DeclineHint.SetLabel(DeclineText.text);
		AddDisposable(m_AcceptHint.Bind(m_InputLayer.AddButton(delegate
		{
			base.ViewModel.Confirm();
		}, 8)));
		m_AcceptHint.SetLabel(AcceptText.text);
		AddDisposable(GamePad.Instance.PushLayer(m_InputLayer));
	}
}
