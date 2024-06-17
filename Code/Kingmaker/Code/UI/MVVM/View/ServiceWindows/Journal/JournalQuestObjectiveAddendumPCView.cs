using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalQuestObjectiveAddendumPCView : JournalQuestObjectiveAddendumBaseView
{
	[SerializeField]
	private Image m_HintPlace;

	[SerializeField]
	private float m_DefaultFontSize = 20f;

	protected override void SetupState()
	{
		base.SetupState();
		if (m_HintPlace != null)
		{
			m_HintPlace.SetHint(GetHintText());
		}
	}

	protected override void SetupDescription()
	{
		base.SetupDescription();
		m_Description.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
		m_EtudeCounter.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
		if (m_Destination != null)
		{
			m_Destination.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
		}
	}
}
