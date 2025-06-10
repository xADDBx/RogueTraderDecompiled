using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;
using Kingmaker.UI.Common;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Alignment.AlignmentHistory;

public class CharInfoChoicesMadeView : CharInfoComponentView<CharInfoAlignmentHistoryVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private WidgetListMVVM m_WidgetList;

	[SerializeField]
	private CharInfoSoulMarkShiftRecordPCView m_SoulMarkShiftRecordView;

	[Header("No Status Effects")]
	[SerializeField]
	private GameObject m_NoStatusContainer;

	[SerializeField]
	private TextMeshProUGUI m_NoStatusEffectsLabel;

	private AccessibilityTextHelper m_TextHelper;

	public override void Initialize()
	{
		base.Initialize();
		m_TextHelper = new AccessibilityTextHelper(m_Label, m_NoStatusEffectsLabel);
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Label.text = UIStrings.Instance.CharacterSheet.History;
		m_TextHelper.UpdateTextSize();
		m_NoStatusContainer.Or(null)?.gameObject.SetActive(base.ViewModel.SoulMarkShiftsHistory.Count == 0);
		m_NoStatusEffectsLabel.text = UIStrings.Instance.CharacterSheet.EmptySoulMarkShiftsDesc.Text;
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		DrawEntities();
	}

	private void DrawEntities()
	{
		m_WidgetList.DrawEntries(base.ViewModel.SoulMarkShiftsHistory, m_SoulMarkShiftRecordView);
	}
}
