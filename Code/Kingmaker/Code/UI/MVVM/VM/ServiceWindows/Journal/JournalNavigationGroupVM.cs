using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Enums;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Journal;

public class JournalNavigationGroupVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly ReactiveProperty<bool> IsSelected = new ReactiveProperty<bool>();

	public readonly string Title;

	public readonly List<JournalQuestVM> Quests;

	private readonly QuestGroup m_Group;

	public bool HasActiveQuests => Quests.Any((JournalQuestVM q) => q.IsActive);

	public bool IsCollapse
	{
		get
		{
			return m_Group.IsCollapse;
		}
		set
		{
			m_Group.IsCollapse = value;
		}
	}

	public JournalNavigationGroupVM(QuestGroupId groupId, IEnumerable<Quest> quests, ReactiveProperty<Quest> selectedQuest, Action<Quest> selectQuest)
	{
		m_Group = Game.Instance.BlueprintRoot.Quests.GetGroup(groupId);
		Title = m_Group.Name;
		AddDisposable(selectedQuest.Subscribe(OnSelectedQuestChange));
		if (quests.Any((Quest q) => q == selectedQuest.Value))
		{
			IsCollapse = false;
		}
		Quests = new List<JournalQuestVM>();
		foreach (Quest quest in quests)
		{
			Quests.Add(new JournalQuestVM(quest, selectedQuest, selectQuest));
		}
	}

	protected override void DisposeImplementation()
	{
		Quests.ForEach(delegate(JournalQuestVM quest)
		{
			quest.Dispose();
		});
		Quests.Clear();
	}

	private void OnSelectedQuestChange(Quest quest)
	{
		if (quest != null)
		{
			bool flag = Quests.Any((JournalQuestVM questVM) => questVM.Quest == quest);
			if (flag && IsCollapse)
			{
				IsCollapse = false;
			}
			IsSelected.Value = flag;
		}
	}
}
