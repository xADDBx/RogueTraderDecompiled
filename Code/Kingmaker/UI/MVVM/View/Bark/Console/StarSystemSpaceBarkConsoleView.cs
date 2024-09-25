using Kingmaker.UI.MVVM.View.Bark.Base;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Rewired;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Bark.Console;

public class StarSystemSpaceBarkConsoleView : StarSystemSpaceBarkBaseView
{
	[SerializeField]
	private ConsoleHint m_ShowLastMessageHint;

	private readonly ReactiveProperty<bool> m_MessageIsHidden = new ReactiveProperty<bool>();

	protected override void AddSystemMapInputImpl(InputLayer inputLayer)
	{
		AddDisposable(m_ShowLastMessageHint.Bind(inputLayer.AddButton(delegate
		{
			ShowLastMessage();
		}, 14, m_MessageIsHidden, InputActionEventType.ButtonJustLongPressed)));
	}

	protected override void ShowAnimatedImpl()
	{
		m_MessageIsHidden.Value = false;
	}

	protected override void ResetAnimationImpl()
	{
		m_MessageIsHidden.Value = true;
	}
}
