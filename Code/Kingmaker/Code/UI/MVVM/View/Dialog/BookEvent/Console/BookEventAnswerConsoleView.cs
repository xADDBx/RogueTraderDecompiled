using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Dialog.BookEvent.Console;

public class BookEventAnswerConsoleView : BookEventAnswerView
{
	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_AnswerText.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontSizeMultiplier;
	}
}
