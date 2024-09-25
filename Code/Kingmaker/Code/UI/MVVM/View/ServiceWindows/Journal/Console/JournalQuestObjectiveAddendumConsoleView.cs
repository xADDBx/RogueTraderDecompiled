using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalQuestObjectiveAddendumConsoleView : JournalQuestObjectiveAddendumBaseView
{
	[SerializeField]
	private float m_DefaultConsoleFontSize = 20f;

	protected override void SetupDescription()
	{
		base.SetupDescription();
		m_Description.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
		m_EtudeCounter.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
		if (m_Destination != null)
		{
			m_Destination.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
		}
	}
}
