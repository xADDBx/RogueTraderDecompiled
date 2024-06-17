using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Enums;
using Owlcat.Runtime.UI.ConsoleTools;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Console;

public class JournalNavigationConsoleView : JournalNavigationBaseView
{
	[SerializeField]
	private JournalNavigationGroupConsoleView m_NavigationGroupViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementConsoleView m_NavigationRumorViewPrefab;

	[SerializeField]
	private JournalNavigationGroupElementConsoleView m_NavigationOrderViewPrefab;

	[Header("Paper")]
	[SerializeField]
	private RectTransform m_PaperTransform;

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
		ScrollToRect();
	}

	public void ScrollMenu(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}

	public List<IConsoleEntity> GetNavigationEntities()
	{
		List<IConsoleEntity> list = new List<IConsoleEntity>();
		if (base.WidgetList.Entries == null)
		{
			return list;
		}
		foreach (MonoBehaviour entry in base.WidgetList.Entries)
		{
			if (entry is JournalNavigationGroupConsoleView journalNavigationGroupConsoleView)
			{
				list.AddRange(journalNavigationGroupConsoleView.GetSelectableEntities());
			}
			else if (entry is JournalNavigationGroupElementConsoleView item)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public void OnPrevActiveTab()
	{
		base.ViewModel.OnPrevActiveTab();
	}

	public void OnNextActiveTab()
	{
		base.ViewModel.OnNextActiveTab();
	}
}
