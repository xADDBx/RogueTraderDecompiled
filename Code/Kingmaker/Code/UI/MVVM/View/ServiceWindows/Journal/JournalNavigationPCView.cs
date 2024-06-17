using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Enums;
using Owlcat.Runtime.UI.Controls.Toggles;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalNavigationPCView : JournalNavigationBaseView
{
	[SerializeField]
	private JournalNavigationGroupPCView m_NavigationGroupViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementPCView m_NavigationRumorViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementPCView m_NavigationOrderViewPrefab;

	[SerializeField]
	private OwlcatToggle m_ShowCompleteToggle;

	[SerializeField]
	private TextMeshProUGUI m_ShowCompleteLabel;

	[Header("Paper")]
	[SerializeField]
	private RectTransform m_PaperTransform;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_ShowCompleteToggle.Set(base.ShowCompleted);
		m_ShowCompleteLabel.text = UIStrings.Instance.QuesJournalTexts.ShowCompletedQuests;
		AddDisposable(m_ShowCompleteToggle.IsOn.Subscribe(base.OnShowCompletedToggleChanged));
	}

	public override void DrawEntities()
	{
		base.DrawEntities();
		if (base.ViewModel.ActiveTab.Value == JournalTab.Quests)
		{
			JournalNavigationGroupVM[] vmCollection = (base.ShowCompleted ? base.ViewModel.NavigationGroups.ToArray() : base.ViewModel.NavigationGroups.Where((JournalNavigationGroupVM q) => q.HasActiveQuests).ToArray());
			base.WidgetList.DrawEntries(vmCollection, m_NavigationGroupViewPrefab);
		}
		if (base.ViewModel.ActiveTab.Value == JournalTab.Rumors)
		{
			IEnumerable<JournalQuestVM> enumerable = (base.ShowCompleted ? base.ViewModel.Rumors.Where((JournalQuestVM q) => q.Quest.Blueprint.Type == QuestType.Rumour) : base.ViewModel.Rumors.Where((JournalQuestVM q) => q.IsActive && q.Quest.Blueprint.Type == QuestType.Rumour));
			IEnumerable<JournalQuestVM> enumerable2 = (base.ShowCompleted ? base.ViewModel.Rumors.Where((JournalQuestVM q) => q.Quest.Blueprint.Type == QuestType.RumourAboutUs) : base.ViewModel.Rumors.Where((JournalQuestVM q) => q.IsActive && q.Quest.Blueprint.Type == QuestType.RumourAboutUs));
			m_RumoursTitleGO.SetActive(enumerable.Any());
			m_RumoursAboutUsButtonGO.SetActive(enumerable2.Any());
			JournalQuestVM[] vmCollection2 = enumerable.Concat(enumerable2).ToArray();
			base.WidgetList.DrawEntries(vmCollection2, m_NavigationRumorViewPrefab);
			m_RumoursTitleGO.transform.SetSiblingIndex(0);
			m_RumoursAboutUsButtonGO.transform.SetSiblingIndex(enumerable.Count() + 1);
		}
		if (base.ViewModel.ActiveTab.Value == JournalTab.Orders)
		{
			JournalQuestVM[] vmCollection3 = (base.ShowCompleted ? base.ViewModel.Orders.ToArray() : base.ViewModel.Orders.Where((JournalQuestVM q) => q.IsActive).ToArray());
			base.WidgetList.DrawEntries(vmCollection3, m_NavigationOrderViewPrefab);
		}
		m_PaperTransform.SetSiblingIndex(0);
		m_EmptyListObject.gameObject.SetActive(base.WidgetList.Entries == null || !base.WidgetList.Entries.Any());
		ScrollToTop();
	}
}
