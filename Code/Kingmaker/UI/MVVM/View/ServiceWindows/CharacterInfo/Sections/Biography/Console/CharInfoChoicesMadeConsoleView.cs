using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.ConsoleTools.GamepadInput;
using Owlcat.Runtime.UI.ConsoleTools.HintTool;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using Rewired;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Biography.Console;

public class CharInfoChoicesMadeConsoleView : CharInfoChoicesMadeView, ICharInfoComponentConsoleView, ICharInfoComponentView
{
	[Header("Console")]
	[SerializeField]
	private ScrollRectExtended m_ScrollView;

	[SerializeField]
	private ConsoleHint m_ScrollHint;

	public void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget)
	{
		if ((bool)m_ScrollHint && m_ScrollView.content.sizeDelta.y >= m_ScrollView.viewport.sizeDelta.y)
		{
			InputBindStruct inputBindStruct = inputLayer.AddAxis(Scroll, 3);
			AddDisposable(m_ScrollHint.Bind(inputBindStruct));
			AddDisposable(inputBindStruct);
		}
	}

	private void Scroll(InputActionEventData obj, float value)
	{
		m_ScrollView.Scroll(value, smooth: true);
	}

	bool ICharInfoComponentView.get_IsBinded()
	{
		return base.IsBinded;
	}
}
