using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.UI.Common;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalQuestObjectiveConsoleView : JournalQuestObjectiveBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonConsole m_ExpandableCollapseMultiButton;

	[Header("Navigation Group Objects")]
	[SerializeField]
	[UsedImplicitly]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	[UsedImplicitly]
	private JournalQuestObjectiveAddendumConsoleView m_AddendumViewPrefab;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 21f;

	protected override void BindViewImplementation()
	{
		SetupHeader();
		base.BindViewImplementation();
		DrawEntities();
		SetTextFontSize();
	}

	private void SetTextFontSize()
	{
		m_Description.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
		m_EtudeCounter.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
		m_Destination.fontSize = m_DefaultConsoleFontSize * base.ViewModel.FontMultiplier;
	}

	public override void SetupHeader()
	{
		SetHeaderExpandableButtonSettings();
		base.SetupHeader();
	}

	private void SetHeaderExpandableButtonSettings()
	{
		if (!(m_ExpandableCollapseMultiButton == null))
		{
			m_ExpandableCollapseMultiButton.LayerIsOffAlways = base.ViewModel.IsCompleted || base.ViewModel.IsFailed;
			m_ExpandableCollapseMultiButton.LayerIsOnAlways = !base.ViewModel.IsCompleted && !base.ViewModel.IsFailed;
			if (base.ViewModel.IsCompleted || base.ViewModel.IsFailed)
			{
				m_ExpandableCollapseMultiButton.SetActiveLayer(isOn: false);
				m_ExpandableCollapseMultiButton.SetFocus(value: false);
			}
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Addendums.ToArray(), m_AddendumViewPrefab);
	}
}
