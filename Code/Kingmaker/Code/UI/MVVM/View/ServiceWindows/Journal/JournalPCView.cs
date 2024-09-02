using System.Collections.Generic;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal.Base;
using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;
using Kingmaker.Enums;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models.SettingsUI;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.Journal;

public class JournalPCView : JournalBaseView
{
	[SerializeField]
	private JournalNavigationPCView m_NavigationView;

	[SerializeField]
	private JournalQuestPCView m_QuestView;

	[SerializeField]
	private JournalRumourPCView m_RumourView;

	[SerializeField]
	private JournalOrderPCView m_OrderView;

	[SerializeField]
	private FlexibleLensSelectorView m_SelectorView;

	public override void Initialize()
	{
		m_NavigationView.Initialize();
		m_QuestView.Initialize();
		m_RumourView.Initialize();
		m_OrderView.Initialize();
		base.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_NavigationView.Bind(base.ViewModel.Navigation);
		m_SelectorView.Bind(base.ViewModel.Selector);
		AddDisposable(base.ViewModel.UpdateView.Subscribe(OnSelectedQuestChange));
		OnSelectedQuestChange(base.ViewModel.SelectedQuest.Value);
		UpdateNavigation();
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.PrevTab.name, delegate
		{
			m_NavigationView.OnPrevActiveTab();
			m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
		}));
		AddDisposable(Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.NextTab.name, delegate
		{
			m_NavigationView.OnNextActiveTab();
			m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
		}));
	}

	private void UpdateNavigation()
	{
		if (!JournalHelper.HasCurrentQuest)
		{
			JournalHelper.ChangeCurrentQuest(m_NavigationView.GetCurrentQuest());
		}
	}

	private void OnSelectedQuestChange(Quest selectedQuest)
	{
		m_SelectorView.ChangeTab((int)m_NavigationView.GetActiveTab());
		if (selectedQuest != null && selectedQuest.Blueprint.Group == QuestGroupId.Rumours)
		{
			TryBindQuestView(base.ViewModel.Navigation.Rumors);
		}
		if (selectedQuest != null && selectedQuest.Blueprint.Group == QuestGroupId.Orders)
		{
			TryBindQuestView(base.ViewModel.Navigation.Orders);
			return;
		}
		foreach (JournalNavigationGroupVM navigationGroup in base.ViewModel.Navigation.NavigationGroups)
		{
			if (TryBindQuestView(navigationGroup.Quests))
			{
				break;
			}
		}
	}

	private bool TryBindQuestView(IEnumerable<JournalQuestVM> quests)
	{
		foreach (JournalQuestVM quest in quests)
		{
			if (quest.IsSelected.Value)
			{
				BaseJournalItemPCView baseJournalItemPCView = (quest.IsRumour ? m_RumourView : (quest.IsOrder ? ((BaseJournalItemPCView)m_OrderView) : ((BaseJournalItemPCView)m_QuestView)));
				BaseJournalItemPCView baseJournalItemPCView2 = (quest.IsRumour ? m_QuestView : (quest.IsOrder ? ((BaseJournalItemPCView)m_RumourView) : ((BaseJournalItemPCView)m_OrderView)));
				BaseJournalItemPCView obj = (quest.IsRumour ? m_OrderView : (quest.IsOrder ? ((BaseJournalItemPCView)m_QuestView) : ((BaseJournalItemPCView)m_RumourView)));
				baseJournalItemPCView.Bind(quest);
				baseJournalItemPCView2.Unbind();
				obj.Unbind();
				return true;
			}
		}
		return false;
	}
}
