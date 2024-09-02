using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Enums;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;

public class JournalNavigationVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<JournalTab> ActiveTab = new ReactiveProperty<JournalTab>();

	public readonly List<JournalNavigationGroupVM> NavigationGroups;

	public readonly Action<Quest> SelectQuest;

	public readonly List<JournalQuestVM> Rumors = new List<JournalQuestVM>();

	public readonly List<JournalQuestVM> Orders = new List<JournalQuestVM>();

	public bool CannotAccessContracts => Game.Instance.Player.CannotAccessContracts.Value;

	public JournalNavigationVM(IEnumerable<Quest> quests, ReactiveProperty<Quest> selectedQuest, Action<Quest> selectQuest)
	{
		JournalNavigationVM journalNavigationVM = this;
		SelectQuest = selectQuest;
		Dictionary<QuestGroupId, List<Quest>> dictionary = new Dictionary<QuestGroupId, List<Quest>>();
		foreach (Quest quest in quests)
		{
			if (!dictionary.TryGetValue(quest.Blueprint.Group, out var value2))
			{
				value2 = new List<Quest>();
				dictionary.Add(quest.Blueprint.Group, value2);
			}
			value2.Add(quest);
		}
		NavigationGroups = new List<JournalNavigationGroupVM>();
		foreach (QuestGroup group in Game.Instance.BlueprintRoot.Quests.Groups.OrderBy((QuestGroup g) => g.Order).ToList())
		{
			if (!dictionary.ContainsKey(group.Id))
			{
				continue;
			}
			KeyValuePair<QuestGroupId, List<Quest>> keyValuePair = dictionary.First((KeyValuePair<QuestGroupId, List<Quest>> d) => d.Key == group.Id);
			if (group.Id == QuestGroupId.Rumours)
			{
				keyValuePair.Value.ForEach(delegate(Quest quest)
				{
					journalNavigationVM.Rumors.Add(new JournalQuestVM(quest, selectedQuest, selectQuest));
				});
			}
			else if (group.Id == QuestGroupId.Orders)
			{
				keyValuePair.Value.ForEach(delegate(Quest quest)
				{
					journalNavigationVM.Orders.Add(new JournalQuestVM(quest, selectedQuest, selectQuest));
				});
			}
			else
			{
				NavigationGroups.Add(new JournalNavigationGroupVM(keyValuePair.Key, keyValuePair.Value, selectedQuest, selectQuest));
			}
		}
		AddDisposable(selectedQuest.Subscribe(delegate(Quest value)
		{
			JournalTab journalTab = value?.Blueprint.Group switch
			{
				QuestGroupId.Rumours => JournalTab.Rumors, 
				QuestGroupId.Orders => JournalTab.Orders, 
				_ => JournalTab.Quests, 
			};
			if (selectedQuest != null && journalNavigationVM.ActiveTab.Value != journalTab)
			{
				journalNavigationVM.SetActiveTab(journalTab);
			}
		}));
	}

	protected override void DisposeImplementation()
	{
		NavigationGroups.ForEach(delegate(JournalNavigationGroupVM group)
		{
			group.Dispose();
		});
		NavigationGroups.Clear();
		Rumors.ForEach(delegate(JournalQuestVM rumor)
		{
			rumor.Dispose();
		});
		Rumors.Clear();
		Orders.ForEach(delegate(JournalQuestVM order)
		{
			order.Dispose();
		});
		Orders.Clear();
	}

	public bool CheckReadyToCompleteOrders()
	{
		return Orders.Any((JournalQuestVM o) => ActiveTab.Value != JournalTab.Orders && o.CanCompleteOrder && !o.IsOrderCompleted.Value && o.Quest.State != QuestState.Completed);
	}

	public void SetActiveTab(JournalTab activeTab)
	{
		if (ActiveTab.Value != activeTab && (activeTab != JournalTab.Orders || !CannotAccessContracts))
		{
			ActiveTab.Value = activeTab;
		}
	}

	public void OnNextActiveTab()
	{
		JournalTab journalTab = ActiveTab.Value + 1;
		if (journalTab >= JournalTab.Quests && journalTab <= JournalTab.Orders)
		{
			SetActiveTab(journalTab);
		}
	}

	public void OnPrevActiveTab()
	{
		JournalTab journalTab = ActiveTab.Value - 1;
		if (journalTab >= JournalTab.Quests && journalTab <= JournalTab.Orders)
		{
			SetActiveTab(journalTab);
		}
	}
}
