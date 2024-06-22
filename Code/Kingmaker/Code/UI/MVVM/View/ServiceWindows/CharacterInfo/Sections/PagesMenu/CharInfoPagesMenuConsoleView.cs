using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.PagesMenu;

public class CharInfoPagesMenuConsoleView : CharInfoPagesMenuPCView
{
	[Header("Hints")]
	[SerializeField]
	private ConsoleHint m_PreviousFilterHint;

	[SerializeField]
	private ConsoleHint m_NextFilterHint;

	public void AddHints(InputLayer inputLayer, IReadOnlyReactiveProperty<bool> enabledHints = null)
	{
		AddDisposable(m_PreviousFilterHint.Bind(inputLayer.AddButton(delegate
		{
			SelectPrev();
		}, 14, enabledHints)));
		AddDisposable(m_NextFilterHint.Bind(inputLayer.AddButton(delegate
		{
			SelectNext();
		}, 15, enabledHints)));
	}
}
