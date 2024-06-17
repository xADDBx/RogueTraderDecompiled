using Kingmaker.UI.MVVM.View.CharGen.Common.Phases.Attributes;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UniRx;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.CharGen.Console.Phases.Attributes;

public class CharGenAttributesPhasePantographItemConsoleView : CharGenAttributesPhasePantographItemView
{
	[SerializeField]
	private ConsoleHint m_MinusHint;

	[SerializeField]
	private ConsoleHint m_PlusHint;

	private bool m_HintsAdded;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_HintsAdded = false;
		AddHints(base.ViewModel.IsMainCharacter.Value);
		AddDisposable(base.ViewModel.CheckCoopControls.Subscribe(AddHints));
	}

	private void AddHints(bool isMainCharacter)
	{
		if (isMainCharacter && !m_HintsAdded)
		{
			AddDisposable(DelayedInvoker.InvokeInFrames(delegate
			{
				AddDisposable(m_MinusHint.BindCustomAction(4, GamePad.Instance.CurrentInputLayer, base.ViewModel.CanRetreat));
				AddDisposable(m_PlusHint.BindCustomAction(5, GamePad.Instance.CurrentInputLayer, base.ViewModel.CanAdvance));
			}, 5));
			m_HintsAdded = true;
		}
	}
}
