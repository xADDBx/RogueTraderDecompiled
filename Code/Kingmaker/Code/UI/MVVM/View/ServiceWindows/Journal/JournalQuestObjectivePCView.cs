using JetBrains.Annotations;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalQuestObjectivePCView : JournalQuestObjectiveBaseView
{
	[SerializeField]
	[UsedImplicitly]
	private ExpandableCollapseMultiButtonPC m_ExpandableCollapseMultiButton;

	[Header("Navigation Group Objects")]
	[SerializeField]
	[UsedImplicitly]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	[UsedImplicitly]
	private JournalQuestObjectiveAddendumPCView m_AddendumViewPrefab;

	[SerializeField]
	private Image m_HintPlace;

	[SerializeField]
	private float m_DefaultFontSize = 21f;

	protected override void BindViewImplementation()
	{
		SetupHeader();
		base.BindViewImplementation();
		DrawEntities();
		SetTextFontSize();
	}

	private void SetTextFontSize()
	{
		m_Description.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
		m_EtudeCounter.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
		m_Destination.fontSize = m_DefaultFontSize * base.ViewModel.FontMultiplier;
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
			m_ExpandableCollapseMultiButton.SetValue(!base.ViewModel.IsCompleted && !base.ViewModel.IsFailed, isImmediately: true);
			m_ExpandableCollapseMultiButton.LayerIsOffAlways = base.ViewModel.IsCompleted || base.ViewModel.IsFailed;
			m_ExpandableCollapseMultiButton.LayerIsOnAlways = !base.ViewModel.IsCompleted && !base.ViewModel.IsFailed;
			if (base.ViewModel.IsCompleted || base.ViewModel.IsFailed)
			{
				m_ExpandableCollapseMultiButton.SetFocus(value: false);
			}
		}
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.Addendums.ToArray(), m_AddendumViewPrefab);
	}

	protected override void SetupState()
	{
		base.SetupState();
		if (m_HintPlace != null)
		{
			m_HintPlace.SetHint(GetHintText());
		}
	}
}
